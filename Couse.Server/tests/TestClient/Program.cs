using System.Net.Sockets;
using System.Text;

namespace TestClient;

class Program
{
    private static Guid _receiverId = Guid.Parse("024EFFDA-2A5C-42CE-8A65-ADDBECE9DB0D");
    
    // receiver client
    private static async Task Main(string[] args)
    {
        var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            await clientSocket.ConnectAsync("127.0.0.1", 5005);
            
            // отправим на сервер свой Id чтобы он знал кто к нему подключился
            await clientSocket.SendAsync(Encoding.UTF8.GetBytes(_receiverId.ToString()), SocketFlags.None);
        }
        catch (Exception e)
        {
            Console.WriteLine("Не удалось подключиться к серверу. {0}", e);
        }
        
        _ = Task.Run(async () =>
        {
            var buffer = new byte[1024];
            while (true)
            {
                await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
                Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, buffer.Length));
            }
        });
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadLine();
    }
}