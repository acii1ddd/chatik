using System.Xml.Serialization;

namespace Course.Contracts.Contracts.Serialize;

[XmlRoot("Fault", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class Fault
{
    [XmlElement("faultcode")]
    public string Code { get; set; } = string.Empty;

    [XmlElement("faultstring")]
    public string Message { get; set; } = string.Empty;
}
