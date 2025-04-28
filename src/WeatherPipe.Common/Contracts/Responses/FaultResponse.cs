using System.Text;
using System.Xml.Serialization;

namespace Course.Contracts.Contracts.Responses;

[XmlRoot("Fault", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class FaultResponse : IResponse
{
    [XmlElement("faultcode")]
    public string Code { get; set; } = string.Empty;

    [XmlElement("faultstring")]
    public string Message { get; set; } = string.Empty;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Получена ошибка от сервера. Операция прервана.");
        sb.AppendLine($"Код ошибки: {Code}");
        sb.AppendLine($"Сообщение: {Message}");
        return sb.ToString();
    }
}
