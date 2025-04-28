using System.Xml.Serialization;

namespace Course.Contracts.Contracts.Responses;


[XmlRoot("WeatherDay", Namespace = "http://tempuri.org/WeatherService")]
public class WeatherDayResponse
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

