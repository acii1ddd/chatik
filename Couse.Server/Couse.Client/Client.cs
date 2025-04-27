using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Couse.Client;

public static class Client
{
    private const string ServerIp = "127.0.0.1";
    private const int Port = 5000;
    private const int BufferSize = 4096;
    
    private static async Task Main(string[] args)
    {
        var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var endPoint = new IPEndPoint(IPAddress.Parse(ServerIp), Port);

        try
        {
            Console.WriteLine($"Connecting to {endPoint}...");
            await clientSocket.ConnectAsync(endPoint);
            Console.WriteLine($"Connected to {endPoint} successfully!.");


            // var soapRequest = CreateRequest(message);
            await clientSocket.SendAsync(Encoding.UTF8.GetBytes("test-soapRequest"), SocketFlags.None);
            Console.WriteLine("\nRequest sent, waiting for response...");

            var buffer = new byte[BufferSize];
            var sb = new StringBuilder();
            while (true)
            {
                var bytesReceived = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
                if (bytesReceived == 0)
                    break; // сервер закрыл соединениеы

                var chunk = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                sb.Append(chunk);


                // if (sb.ToString().Contains("</soap:Envelope>")) 
                //     break;
                if (sb.ToString() == "test-response") 
                    break;
            }

            var response = sb.ToString();
            Console.WriteLine("\nResponse from server: {0}", response);
        }
        catch (SocketException e)
        {
            Console.WriteLine(e);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            if (clientSocket.Connected)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
            }
            clientSocket.Close();
            Console.WriteLine("Connection closed.");
        }
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

//     private static string CreateRequest(string message)
//     {
//         return $"""
//                 <?xml version="1.0"?>
//                 <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
//                     <soap:Header>
//                         <SenderId>{_senderId.ToString()}</SenderId>
//                         <ReceiverId>{_receiverId.ToString()}</ReceiverId>   
//                     </soap:Header>
//                     <soap:Body>
//                         <SendMessage>
//                             <Message>{message}</Message>
//                         </SendMessage>
//                     </soap:Body>
//                 </soap:Envelope>
//                 """;
//     }
}