using Course.Contracts.Contracts.Responses.ForToday;
using WeatherPipe.Client.Models;

namespace WeatherPipe.Client;

public static class Printer
{
    public static void PrintForecastForToday(GetWeatherForTodayResponse weatherForToday, UserData userData)
    {
        Console.WriteLine("=== Погода на сегодня ===");
        Console.WriteLine($"Страна: {userData.Country}");
        Console.WriteLine($"Город: {userData.City}");
        Console.WriteLine($"Координаты: {weatherForToday.Latitude}°, {weatherForToday.Longitude}°");
        Console.WriteLine($"Часовой пояс: {weatherForToday.TimeZone}");
        Console.WriteLine($"Время получения последних данных о погоде: {weatherForToday.Time} (обновление каждые {weatherForToday?.Interval / 60} минут)");
        Console.WriteLine($"Температура: {weatherForToday?.Temperature}°C");
        Console.WriteLine($"Скорость ветра: {weatherForToday?.WindSpeed} км/ч");
        Console.WriteLine($"Направление ветра: {ConvertWindDirection(weatherForToday!.WindDirection)}");
        Console.WriteLine(weatherForToday.IsDay == 1
            ? "☀️ День: солнце над горизонтом"
            : "🌙 Ночь: солнце зашло за горизонт"
        );
    }
    
    private static string ConvertWindDirection(double windDirection)
    {
        return windDirection switch
        {
            >= 0 and < 45 => "Север",
            >= 45 and < 90 => "Северо-восток",
            >= 90 and < 135 => "Восток",
            >= 135 and < 180 => "Юго-восток",
            >= 180 and < 225 => "Юг",
            >= 225 and < 270 => "Юго-запад",
            >= 270 and < 315 => "Запад",
            _ => "Северо-запад"
        };
    }
}

