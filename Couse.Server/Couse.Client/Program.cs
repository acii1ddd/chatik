using System.Net.Sockets;
using System.Text;

namespace Couse.Client;

class Program
{
    static async Task Main(string[] args)
    {
        const string serverIp = "127.0.0.1";
        const int port = 5005;
        
        const string message = "Hello, bro";
        var soapRequest = CreateRequest(message);
        
        await SendMessage(serverIp, port, message);
    }

    private static async Task SendMessage(string serverIp, int port, string message)
    {
        try
        {
            using (var client = new TcpClient(serverIp, port))
            {
                await client.ConnectAsync(serverIp, port);
                using var stream = client.GetStream();

                byte[] requestData = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(requestData, 0, requestData.Length);

                // Читаем ответ
                byte[] responseBuffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

                Console.WriteLine("Ответ сервера:");
                Console.WriteLine(response);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static string CreateRequest(string message)
    {
        return $"""
                <?xml version="1.0"?>
                <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                    <soap:Body>
                        <SendMessage>
                            <Message>{message}</Message>
                        </SendMessage>
                    </soap:Body>
                </soap:Envelope>
                """;
    }
}