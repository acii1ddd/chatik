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
}