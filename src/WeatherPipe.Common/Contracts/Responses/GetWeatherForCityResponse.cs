using System.Text;
using System.Xml.Serialization;

namespace Course.Contracts.Contracts.Responses;

[XmlRoot("GetWeatherForCityResponse", Namespace = "http://tempuri.org/WeatherService")]
public class GetWeatherForCityResponse : IResponse
{
    [XmlElement("City")]
    public string City { get; set; } = string.Empty;
    
    [XmlElement("Country")]
    public string Country { get; set; } = string.Empty;
    
    [XmlElement("Latitude")]
    public double Latitude { get; set; }
    
    [XmlElement("Longitude")]
    public double Longitude { get; set; }

    [XmlElement("DailyForecast")]
    public List<WeatherDayResponse> DailyForecast { get; set; } = [];
    
    public override string ToString()
    {
        var forecastDetails = new StringBuilder();
        forecastDetails.AppendLine($"Прогноз погоды для города {City}, {Country} на 2 недели:");
        forecastDetails.AppendLine($"Координаты: широта {Latitude}, долгота {Longitude}\n");

        foreach (var day in DailyForecast)
        {
            forecastDetails.AppendLine($"📅 {day.Date}:");
            forecastDetails.AppendLine($"- 🌡️ Температура: от {(day.TempMin >= 0 ? "+" : "")}{day.TempMin}°C до {(day.TempMax >= 0 ? "+" : "")}{day.TempMax}°C");
            forecastDetails.AppendLine($"- 🌧️ Осадки: {day.Precipitation}\n");
        }

        return forecastDetails.ToString();
    }
}
