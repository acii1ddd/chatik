using System.Xml.Serialization;

namespace Course.Contracts.Contracts;


[XmlRoot("WeatherDay", Namespace = "http://tempuri.org/WeatherService")]
public class WeatherDay
{
    [XmlElement("Date")]
    public string Date { get; set; } = string.Empty;

    [XmlElement("TempMin")]
    public double TempMin { get; set; }

    [XmlElement("TempMax")]
    public double TempMax { get; set; }

    [XmlElement("Precipitation")]
    public string Precipitation { get; set; } = string.Empty;
}

[XmlRoot("GetWeatherForCityResponse", Namespace = "http://tempuri.org/WeatherService")]
public class GetWeatherForCityResponse
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
    public List<WeatherDay> DailyForecast { get; set; } = [];
}