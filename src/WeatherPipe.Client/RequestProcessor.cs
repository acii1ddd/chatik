using System.Net;
using System.Net.Sockets;
using System.Text;
using Course.Contracts;

namespace WeatherPipe.Client;

public static class RequestProcessor
{
    private const int BufferSize = 4096;

    public static async Task<string> SendRequestAsync(string serverIp, int port, string xmlRequest)
    {
        var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var endPoint = new IPEndPoint(IPAddress.Parse(serverIp), port);

        try
        {
            //Console.WriteLine($"\nConnecting to {endPoint}...");
            await clientSocket.ConnectAsync(endPoint);
            //Console.WriteLine($"Connected to {endPoint} successfully!.");
            
            //Console.WriteLine("Request: {0}", xmlRequest);
            await clientSocket.SendAsync(Encoding.UTF8.GetBytes(xmlRequest), SocketFlags.None);
            Console.WriteLine("\nЗапрос отправлен, ожидание ответа...\n");
            
            var responseXmlStr = await WaitResponseFromServer(clientSocket);
            //Console.WriteLine("\nResponse from server: {0}", responseXmlStr);

            return responseXmlStr;
        }
        catch (SocketException e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            try
            {
                ConnectionManager.CloseClient(clientSocket);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Warning] Ошибка при Shutdown: {ex.Message}");
            }
        }
    }

    private static async Task<string> WaitResponseFromServer(Socket clientSocket)
    {
        var buffer = new byte[BufferSize];
        var sb = new StringBuilder();
        while (true)
        {
            var bytesReceived = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
            if (bytesReceived == 0)
                break; // сервер закрыл соединение

            var chunk = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
            sb.Append(chunk);
                
            if (sb.ToString() == "</soap:Envelope>")
                break;
        }
        return sb.ToString();
    }
}