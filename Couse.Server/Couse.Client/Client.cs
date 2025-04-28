using System.Net;
using System.Net.Sockets;
using System.Text;
using Course.Contracts;
using Course.Contracts.Contracts;
using Course.Contracts.Contracts.Serialize;

namespace Couse.Client;

public static class Client
{
    private const string ServerIp = "127.0.0.1";
    private const int Port = 5000;
    private const int BufferSize = 4096;
    
    private static async Task Main(string[] args)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Weather Console ===");
            Console.WriteLine("0. Выход");
            Console.WriteLine("1. Погода для заданного региона (2 недели)");
            Console.WriteLine("2. Погода сегодня (в текущем регионе)");
            Console.WriteLine("3. История (когда восход/закат)");
            Console.Write("Выберите пункт: ");
            var choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.Write("Введите название города: ");
                var city = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(city))
                {
                    Console.WriteLine("Город не может быть пустым.");
                    Console.WriteLine("Нажмите любую клавишу...");
                    Console.ReadKey();
                    continue;
                }

                await RequestWeather(city);
                Console.WriteLine("Нажмите любую клавишу...");
                Console.ReadKey();
            }
            else if (choice == "0")
            {
                Console.WriteLine("Выход...");
                break; // выход из приложения
            }
            else
            {
                Console.WriteLine("Неверный пункт меню. Нажмите любую клавишу...");
                Console.ReadKey();
            }
        }
    }

    private static async Task RequestWeather(string city)
    {
        var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var endPoint = new IPEndPoint(IPAddress.Parse(ServerIp), Port);

        try
        {
            Console.WriteLine($"\nConnecting to {endPoint}...");
            await clientSocket.ConnectAsync(endPoint);
            Console.WriteLine($"Connected to {endPoint} successfully!.");
            
            var request = new Envelope<GetWeatherForCityRequest>
            {
                Body = new Body<GetWeatherForCityRequest>
                {
                    Content = new GetWeatherForCityRequest
                    {
                        City = city
                    }
                }
            };
            
            var xmlRequest = XmlHelper.SerializeToXml(request);
            
            //Console.WriteLine("Request: {0}", xmlRequest);
            await clientSocket.SendAsync(Encoding.UTF8.GetBytes(xmlRequest), SocketFlags.None);
            Console.WriteLine("\nЗапрос отправлен, ожидание ответа...\n");

            var buffer = new byte[BufferSize];
            var sb = new StringBuilder();
            while (true)
            {
                var bytesReceived = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
                if (bytesReceived == 0)
                    break; // серве р закрыл соединение

                var chunk = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                sb.Append(chunk);
                
                if (sb.ToString() == "</soap:Envelope>")
                    break;
            }

            var responseXmlStr = sb.ToString();
            //Console.WriteLine("\nResponse from server: {0}", responseXmlStr);
            
            if (responseXmlStr.Contains("<Fault>"))
            {
                var faultEnvelope = XmlHelper.XmlDeserialize<Fault>(responseXmlStr);
                Console.WriteLine("Получена ошибка от сервера. Операция прервана.");
                Console.WriteLine($"Код ошибки: {faultEnvelope?.Body.Content.Code}");
                Console.WriteLine($"Сообщение: {faultEnvelope?.Body.Content.Message}");
                return;
            }
            
            var weatherResponseEnvelope = XmlHelper.XmlDeserialize<GetWeatherForCityResponse>(responseXmlStr);
            var weatherResponse = weatherResponseEnvelope?.Body.Content;
            
            Console.WriteLine($"Прогноз погоды для города {weatherResponse?.City}, {weatherResponse?.Country} на 2 недели:");
            Console.WriteLine($"Координаты: широта {weatherResponse?.Latitude}, долгота {weatherResponse?.Longitude}\n");
            foreach (var day in weatherResponse?.DailyForecast!)
            {
                Console.WriteLine($"📅 {day.Date}:");
                Console.WriteLine($"- 🌡️ Температура: от {(day.TempMin >= 0 ? "+" : "")}{day.TempMin}°C до {(day.TempMax >= 0 ? "+" : "")}{day.TempMax}°C");
                Console.WriteLine($"- 🌧️ Осадки: {day.Precipitation}\n");
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine(e);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            if (clientSocket.Connected)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
            }
            clientSocket.Close();
            Console.WriteLine("Connection closed.");
        }
    }
}