using System.Xml.Serialization;

namespace Course.Contracts.Contracts.Serialize;

[XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class Envelope<T> where T : class
{
    [XmlElement("Body")]
    public required Body<T> Body { get; set; }
}
