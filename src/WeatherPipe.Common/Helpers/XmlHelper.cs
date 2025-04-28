using System.Xml.Serialization;
using Course.Contracts.Contracts.Serialize;

namespace Course.Contracts.Helpers;

public static class XmlHelper
{
    public static string SerializeToXml<T>(T envelope)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var sw = new StringWriter();
        serializer.Serialize(sw, envelope);
        var xmlRequest = sw.ToString();
        return xmlRequest;
    }
    
    public static Envelope<T>? XmlDeserialize<T>(string requestXmlString) where T : class
    {
        if (string.IsNullOrEmpty(requestXmlString))
        {
            Console.WriteLine("Получена пустая строка XML запроса");
            return null;
        }
        
        try
        {
            var serializer = new XmlSerializer(typeof(Envelope<T>));
            using var reader = new StringReader(requestXmlString);
            return (Envelope<T>?)serializer.Deserialize(reader);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка десериализации: {ex.Message}");
            return null;
        }
    }
}