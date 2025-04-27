using System.Xml.Serialization;

namespace Course.Contracts.Contracts;

[XmlRoot("GetWeatherForCityResponse", Namespace = "http://tempuri.org/WeatherService")]
public class GetWeatherForCityResponse
{
    [XmlElement("Temperature")]
    public double Temperature { get; init; }
}