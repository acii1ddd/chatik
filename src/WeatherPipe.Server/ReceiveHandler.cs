using System.Net.Sockets;
using System.Text;
using Course.Contracts;

namespace Couse.API;

internal static class ReceiveHandler
{
    internal static void StartReceive(Socket client, int bufferSize)
    {
        var readEventArg = new SocketAsyncEventArgs();
        readEventArg.SetBuffer(new byte[bufferSize], 0, bufferSize);
        readEventArg.UserToken = client;
        readEventArg.Completed += (s, socketAsyncEventArgs) => ProcessReceive(socketAsyncEventArgs);

        // true - асинхронная операция -> сработает Completed
        // false - синхронная операция
        if (!client.ReceiveAsync(readEventArg))
        {
            ProcessReceive(readEventArg);
        }
    }
    
    private static void ProcessReceive(SocketAsyncEventArgs e)
    {
        if (e.UserToken is null) return;

        // сокет клиента, от которого пришли данные
        var client = (Socket)e.UserToken;

        if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
        {
            if (e.Buffer == null) {
                Console.WriteLine("ERROR: Buffer is null, cannot read data");
                ConnectionManager.CloseClient(client);
                return;
            }
            
            var requestXmlString = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);
            Console.WriteLine($"Получено: {requestXmlString}");
            
            var weatherResponse = RequestProcessor.ProcessRequest(requestXmlString);
            ResponseHandler.HandleResponse(client, e, weatherResponse);
        }
        else
        {
            try
            {
                ConnectionManager.CloseClient(client);
                Console.WriteLine("\nКлиент отключён.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Warning] Ошибка при Shutdown: {ex.Message}");
            }
        }
    }
}