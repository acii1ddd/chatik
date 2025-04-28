using System.Globalization;
using System.Text.Json;
using Course.Contracts.Contracts.Responses;
using Course.Contracts.Contracts.Responses.ForCity;
using Course.Contracts.Contracts.Responses.ForToday;

namespace Couse.API.Services;

internal static class WeatherService
{
    internal static IResponse GetForecastForToday(double latitude, double longitude)
    {
        // логика запроса на weather api
        using var http = new HttpClient();
        var todayWeatherForecastUrl = "https://api.open-meteo.com/v1/forecast" +
                                 $"?latitude={latitude.ToString(CultureInfo.InvariantCulture).Replace(',', '.')}" +
                                 $"&longitude={longitude.ToString(CultureInfo.InvariantCulture).Replace(',', '.')}" +
                                 "&current_weather=true" +
                                 "&timezone=auto";
        
        Console.WriteLine("Запрос в {0}", todayWeatherForecastUrl);
        
        var forecastJsonStr = http.GetStringAsync(todayWeatherForecastUrl).Result;
        
        using var forecastJsonDoc = JsonDocument.Parse(forecastJsonStr);
        var weatherRoot = forecastJsonDoc.RootElement;
        var timeZone = weatherRoot.GetProperty("timezone").GetString() ?? string.Empty;
        
        var currentWeather = weatherRoot.GetProperty("current_weather");
        var time = currentWeather.GetProperty("time").GetString() ?? string.Empty;
        var interval = currentWeather.GetProperty("interval").GetInt32();
        var temperature = currentWeather.GetProperty("temperature").GetDouble();
        var windSpeed = currentWeather.GetProperty("windspeed").GetDouble();
        var windDirection = currentWeather.GetProperty("winddirection").GetDouble();
        var isDay = currentWeather.GetProperty("is_day").GetInt32();

        return new GetWeatherForTodayResponse
        {
            Latitude = latitude,
            Longitude = longitude,
            TimeZone = timeZone,
            Time = time,
            Interval = interval,
            Temperature = temperature,
            WindSpeed = windSpeed,
            WindDirection = windDirection,
            IsDay = isDay
        };
    }
    
    internal static IResponse GetForecastForCity(string city)
    {
        // логика запроса на weather api
        using var http = new HttpClient();
        var geoUrl = $"https://geocoding-api.open-meteo.com/v1/search" +
                     $"?name={Uri.EscapeDataString(city)}" +
                     $"&language=ru" +
                     $"&count=1";
                
        var geoJsonStr = http.GetStringAsync(geoUrl).Result;
        using var geoJsonDoc = JsonDocument.Parse(geoJsonStr);
                
        if (!geoJsonDoc.RootElement.TryGetProperty("results", out var results))
        {
            return new FaultResponse
            {
                Code = "Client",
                Message = "Нет информации о погоде для данного региона"
            };
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
        using var forecastJsonDoc = JsonDocument.Parse(forecastJsonStr);
            
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
            var dateStr = DateTime.Parse(dates[i].GetString()!).ToString("dd MMMM yyyy", new CultureInfo("ru-RU"));

            var tempMin = tempsMin[i].GetDouble();
            var tempMax = tempsMax[i].GetDouble();
            var precipitationStr = precipitations[i].ValueKind == JsonValueKind.Null 
                ? "нет данных" 
                : $"{precipitations[i].GetDouble()} мм";

            weatherResponse.DailyForecast.Add(new WeatherDayResponse
            {
                Date = dateStr,
                TempMin = tempMin,
                TempMax = tempMax,
                Precipitation = precipitationStr
            });
        }

        return weatherResponse;
    }
}