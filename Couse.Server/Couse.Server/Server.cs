using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Course.Contracts;
using Course.Contracts.Contracts;
using Course.Contracts.Contracts.Serialize;

namespace Couse.API;

public static class Server
{
    private const int Port = 5000;
    private const int BufferSize = 1024;

    private static void Main()
    {
        var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Any, Port));
        listener.Listen(100);

        Console.WriteLine($"Сервер слушает на порту {Port}...");
    
        StartAccept(listener);
        
        Console.ReadLine();
    }

    private static void StartAccept(Socket listener)
    {
        var acceptEventArg = new SocketAsyncEventArgs();
        
        // событие на ожидание запроса (сработает, когда соединение будет принято асинхронно)
        acceptEventArg.Completed += (s, e) => ProcessAccept(e, listener);
        
        // false if the I/ O operation completed synchronously (для синхронных подключений)
        if (!listener.AcceptAsync(acceptEventArg))
        {
            ProcessAccept(acceptEventArg, listener);
        }
    }

    private static void ProcessAccept(SocketAsyncEventArgs e, Socket listener)
    {
        // может оказаться, что в очереди на accept уже несколько подключений.
        while (true)
        {
            // сокет принявшегося клиента
            var client = e.AcceptSocket;
            if (client == null) return;
            
            Console.WriteLine("Клиент подключён.");

            var readEventArg = new SocketAsyncEventArgs();
            readEventArg.SetBuffer(new byte[BufferSize], 0, BufferSize);
            readEventArg.UserToken = client;
            readEventArg.Completed += IO_Completed!;

            // true - асинхронная операция -> сработает Completed
            // false - синхронная операция
            if (!client.ReceiveAsync(readEventArg))
            {
                ProcessReceive(readEventArg);
            }

            // accept для следующего клиента
            e.AcceptSocket = null;
            
            // false — значит, в очереди уже было следующее подключение while (continue), чтобы обработать его
            // true — приём следующего клиента запланирован асинхронно → выходим из цикла и ждём, когда сработает
            // событие e.Completed.
            if (!listener.AcceptAsync(e))
            {
                continue;
            }

            break;
        }
    }

    // для всех завершённых I/O-операций на сокетах, связанных с SocketAsyncEventArgs
    private static void IO_Completed(object sender, SocketAsyncEventArgs e)
    {
        switch (e.LastOperation)
        {
            case SocketAsyncOperation.Receive:
                ProcessReceive(e);
                break;
            case SocketAsyncOperation.Send:
                ProcessSend(e);
                break;
        }
    }

    private static void ProcessReceive(SocketAsyncEventArgs e)
    {
        if (e.UserToken is null) return;

        // сокет клиента, от которого пришли данные
        var client = (Socket)e.UserToken;

        if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
        {
            if (e.Buffer == null) {
                Console.WriteLine("ERROR: Buffer is null, cannot read data");
                CloseClient(client);
                return;
            }
            var requestXmlString = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);
            if (string.IsNullOrEmpty(requestXmlString))
            {
                Console.WriteLine("Получен пустой запрос!");
                return;
            }
            
            Console.WriteLine($"Получено: {requestXmlString}");
            
            var envelopeRequest = XmlHelper.XmlDeserialize<GetWeatherForCityRequest>(requestXmlString);
            var getWeatherForCityRequest = envelopeRequest?.Body.Content;
            
            Console.WriteLine($"Город = {getWeatherForCityRequest?.City}");
            
            // логика запроса на weather api
            using var http = new HttpClient();
            var geoUrl = $"https://geocoding-api.open-meteo.com/v1/search" +
                         $"?name={Uri.EscapeDataString(getWeatherForCityRequest?.City ?? string.Empty)}" +
                         $"&language=ru" +
                         $"&count=1";
            
            var geoJsonStr = http.GetStringAsync(geoUrl).Result;
            using var geoJsonDoc = JsonDocument.Parse(geoJsonStr);
            
            if (!geoJsonDoc.RootElement.TryGetProperty("results", out var results))
            {
                SendXmlResponse(client, e, ResponseHelper.GetFaultEnvelope(
                    new Fault
                    {
                        Code = "Client",
                        Message = "Нет информации о погоде для данного региона"
                    })
                );
                return;
            }

            // первый из results
            var firstResult = results[0];
            
            var latitude= firstResult.GetProperty("latitude").GetDouble()
                .ToString(CultureInfo.InvariantCulture).Replace(',', '.');
            
            var longitude = firstResult.GetProperty("longitude").GetDouble()
                .ToString(CultureInfo.InvariantCulture).Replace(',', '.');
            
            var name = firstResult.GetProperty("name").GetString();
            var country = firstResult.GetProperty("country").GetString();
            var admin1 = firstResult.GetProperty("admin1").GetString();
            Console.WriteLine($"Coords: lat={latitude}, lon={longitude}");

            var startDate = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            var endDate = DateTime.UtcNow.Date.AddDays(14).ToString("yyyy-MM-dd");
            
            var weatherForecastUrl = "https://api.open-meteo.com/v1/forecast" +
                                     $"?latitude={latitude}" +
                                     $"&longitude={longitude}" +
                                     "&daily=temperature_2m_min,temperature_2m_max,precipitation_sum" +
                                     "&timezone=auto" +
                                     $"&start_date={startDate}" +
                                     $"&end_date={endDate}";
            
            Console.WriteLine(weatherForecastUrl);
            
            var forecastJsonStr = http.GetStringAsync(weatherForecastUrl).Result;
            using var forecastJsonDoc = System.Text.Json.JsonDocument.Parse(forecastJsonStr);
            
            var weatherRoot = forecastJsonDoc.RootElement;
            var daily = weatherRoot.GetProperty("daily");
            var dates = daily.GetProperty("time").EnumerateArray().ToArray();
            var tempsMin = daily.GetProperty("temperature_2m_min").EnumerateArray().ToArray();
            var tempsMax = daily.GetProperty("temperature_2m_max").EnumerateArray().ToArray();
            var precipitations = daily.GetProperty("precipitation_sum").EnumerateArray().ToArray();
            
            var weatherResponse = new GetWeatherForCityResponse
            {
                City = name!,
                Country = country!,
                Latitude = double.Parse(latitude.Replace('.', ',')),
                Longitude = double.Parse(longitude.Replace('.', ',')),
                DailyForecast = []
            };
            
            // 2 недели (14 дней)
            for (var i = 0; i < 14; i++)
            {
                var dateStr = DateTime.Parse(dates[i].GetString()!).ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("ru-RU"));

                var tempMin = tempsMin[i].GetDouble();
                var tempMax = tempsMax[i].GetDouble();
                var precipitationStr = precipitations[i].ValueKind == JsonValueKind.Null 
                    ? "нет данных" 
                    : $"{precipitations[i].GetDouble()} мм";

                weatherResponse.DailyForecast.Add(new WeatherDay
                {
                    Date = dateStr,
                    TempMin = tempMin,
                    TempMax = tempMax,
                    Precipitation = precipitationStr
                });
            }
            
            SendXmlResponse(client, e, new Envelope<GetWeatherForCityResponse>
            {
                Body = new Body<GetWeatherForCityResponse>
                {
                    Content = weatherResponse
                }
            });
        }
        else
        {
            CloseClient(client);
        }
    }

    private static void SendXmlResponse<T>(Socket client, SocketAsyncEventArgs e, Envelope<T> xmlResponseEnvelope) where T : class
    {
        var xmlResponseStr = XmlHelper.SerializeToXml(xmlResponseEnvelope);
        Console.WriteLine("\nResponse text: {0}", xmlResponseStr);
     
        var responseBytes = Encoding.UTF8.GetBytes(xmlResponseStr);
        e.SetBuffer(responseBytes, 0, responseBytes.Length);

        if (!client.SendAsync(e))
        {
            ProcessSend(e);
        }
    }

    private static void ProcessSend(SocketAsyncEventArgs e)
    {
        if (e.UserToken is null) return;
        var client = (Socket)e.UserToken;

        Console.WriteLine($"[{DateTime.UtcNow:O}] Response sent to {client.RemoteEndPoint}, {e.BytesTransferred} bytes.");
        
        // Готовим новый буфер на чтение
        // e.SetBuffer(new byte[BufferSize], 0, BufferSize);
        // if (!client.ReceiveAsync(e))
        // {
        //     ProcessReceive(e);
        // }
        
        // закрываем соединение — больше не ждём новых запросов по этому сокету
        client.Shutdown(SocketShutdown.Both);
        client.Close();
    }
    
    private static void CloseClient(Socket client)
    {
        Console.WriteLine("\nКлиент отключён.");
        try { client.Shutdown(SocketShutdown.Both); } catch { }
        client.Close();
    }
}