using Course.Contracts.Contracts.Requests;
using Course.Contracts.Contracts.Responses;
using Course.Contracts.Contracts.Serialize;
using Course.Contracts.Helpers;

namespace WeatherPipe.Client;

public static class Client
{
    private const string ServerIp = "127.0.0.1";
    private const int Port = 5000;
    
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

                await GetWeatherForCity(city);
                Console.WriteLine("Нажмите любую клавишу...");
                Console.ReadKey();
            }
            else if (choice == "2")
            {
                //
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

    private static async Task GetWeatherForCity(string city)
    {
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
        
        var xmlRequestStr = XmlHelper.SerializeToXml(request);
        var xmlResponseStr = await RequestProcessor.SendRequestAsync(ServerIp, Port, xmlRequestStr);

        // ошибочный ответ
        if (ErrorHandler.IsFaultResponse(xmlResponseStr))
        {
            var faultEnvelope = XmlHelper.XmlDeserialize<FaultResponse>(xmlResponseStr);
            Console.WriteLine(faultEnvelope?.Body.Content);
            return;
        }
        
        var weatherResponseEnvelope = XmlHelper.XmlDeserialize<GetWeatherForCityResponse>(xmlResponseStr);
        Console.WriteLine(weatherResponseEnvelope?.Body.Content);
    }
}