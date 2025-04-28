using System.Xml.Serialization;

namespace Course.Contracts.Contracts.Requests;

[XmlRoot("GetWeatherForCityRequest", Namespace = "http://tempuri.org/WeatherService")]
public class GetWeatherForCityRequest
{
    
    [XmlElement("RequestType")]
    public string RequestType  { get; init; } = string.Empty;
    
    [XmlElement("City")]
    public string City { get; init; } = string.Empty;
}
