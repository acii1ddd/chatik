using System.Net.Sockets;
using System.Text;

namespace Couse.Client;

class Program
{
    private static  Guid _senderId = Guid.Parse("CDCB9114-2BD6-46BA-A55D-BF35E8DDEB04");
    private static  Guid _receiverId = Guid.Parse("024EFFDA-2A5C-42CE-8A65-ADDBECE9DB0D");
    
    // sender client
    static async Task Main(string[] args)
    {
        const string serverIp = "127.0.0.1";
        const int port = 5005;
        const string message = "Hello, bro";
        
        var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await clientSocket.ConnectAsync(serverIp, port);
        
        var soapRequest = CreateRequest(message);
        await clientSocket.SendAsync(Encoding.UTF8.GetBytes(soapRequest), SocketFlags.None);
        
        // ожидание ответа (сделать бесконечный цикл)
        var buffer = new byte[1024];
        var bytesReceived = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
        if (bytesReceived == 0)
        {
            Console.WriteLine("Empty response");
        }
        
        var response = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
        Console.WriteLine(response);
        clientSocket.Close();
    }

    // private static async Task SendMessageAsync(string serverIp, int port, string message)
    // {
    //     try
    //     {
    //         
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e);
    //     }
    // }

    private static string CreateRequest(string message)
    {
        return $"""
                <?xml version="1.0"?>
                <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                    <soap:Header>
                        <SenderId>{_senderId.ToString()}</SenderId>
                        <ReceiverId>{_receiverId.ToString()}</ReceiverId>   
                    </soap:Header>
                    <soap:Body>
                        <SendMessage>
                            <Message>{message}</Message>
                        </SendMessage>
                    </soap:Body>
                </soap:Envelope>
                """;
    }
}