using System.Xml;
using System.Xml.Serialization;

namespace Course.Contracts.Contracts.Serialize;

// public class Body<T> where T : class
// {
//     [XmlElement("GetWeather", Namespace = "http://tempuri.org/WeatherService")]
//     public required T Content { get; set; }
// }

public class Body<T> : IXmlSerializable where T: class
{
    public T Content { get; set; } = null!;

    public System.Xml.Schema.XmlSchema GetSchema() => null!;

    // должны вернуть объект body
    public void ReadXml(XmlReader reader)
    {
        reader.ReadStartElement();               // <Body>
        Content = (T)new XmlSerializer(typeof(T))
            .Deserialize(reader)!;   // десериализуем объект content по типу T
        reader.ReadEndElement();                 // </Body>
    }
    
    public void WriteXml(XmlWriter writer)
    {
        // пишем сам тег по имени типа T
        var serializer = new XmlSerializer(typeof(T)); // будет оборачивать в тег с данным названием типа
        serializer.Serialize(writer, Content); // сериализуем объект content с ns и пишем в writer
    }
}