using System.Xml.Serialization;

namespace Course.Contracts.Contracts.Requests;

[XmlRoot("GetWeatherForTodayRequest", Namespace = "http://tempuri.org/WeatherService")]
public class GetWeatherForTodayRequest
{
    [XmlElement("RequestType")]
    public string RequestType { get; init; } = string.Empty;
    
    [XmlElement("Latitude")]
    public double Latitude { get; init; }
    
    [XmlElement("Longitude")]
    public double Longitude { get; init; }
}