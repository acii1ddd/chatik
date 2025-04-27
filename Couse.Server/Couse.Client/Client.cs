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
            Console.WriteLine("1. Показать погоду для города");
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
            Console.WriteLine($"Connected to {endPoint} successfully!.\n\n");
            
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
            
            // var serializer = new XmlSerializer(typeof(Envelope<GetWeatherForCityRequest>));
            // await using var sw = new StringWriter();
            // serializer.Serialize(sw, request);
            // var xmlRequest = sw.ToString();
            var xmlRequest = XmlHelper.SerializeToXml(request);
            
            Console.WriteLine("Request: {0}", xmlRequest);
            await clientSocket.SendAsync(Encoding.UTF8.GetBytes(xmlRequest), SocketFlags.None);
            Console.WriteLine("\nRequest sent, waiting for response...");

            var buffer = new byte[BufferSize];
            var sb = new StringBuilder();
            while (true)
            {
                var bytesReceived = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
                if (bytesReceived == 0)
                    break; // сервер закрыл соединение

                var chunk = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                sb.Append(chunk);
                
                if (sb.ToString() == "</soap:Envelope>")
                    break;
            }

            var response = sb.ToString();
            Console.WriteLine("\nResponse from server: {0}", response);
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