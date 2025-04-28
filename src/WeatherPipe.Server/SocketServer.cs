using System.Net;
using System.Net.Sockets;

namespace Couse.API;

internal static class SocketServer
{
    private const int Port = 5000;
    private const int BufferSize = 1024;
    
    internal static void Start()
    {
        var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Any, Port));
        listener.Listen(100);
        Console.WriteLine($"Сервер слушает на порту {Port}...");
        StartAccept(listener);
    }

    private static void StartAccept(Socket listener)
    {
        var acceptEventArg = new SocketAsyncEventArgs();
        
        // событие на ожидание запроса (сработает, когда соединение будет принято асинхронно)
        acceptEventArg.Completed += (s, e) => ProcessAccept(e, listener);
        
        // false if the I/ O operation completed synchronously (для синхронных подключений)
        if (!listener.AcceptAsync(acceptEventArg))
        {
            ProcessAccept(acceptEventArg, listener);
        }
    }
    
    private static void ProcessAccept(SocketAsyncEventArgs e, Socket listener)
    {
        // может оказаться, что в очереди на accept уже несколько подключений.
        while (true)
        {
            // сокет принявшегося клиента
            var client = e.AcceptSocket;
            if (client == null) return;
            
            Console.WriteLine("Клиент подключён.");

            // обрабатываем подключение соответствующего клиента
            ReceiveHandler.StartReceive(client, BufferSize);

            // accept для следующего клиента
            e.AcceptSocket = null;
            
            // false — значит, в очереди уже было следующее подключение while (continue), чтобы обработать его
            // true — приём следующего клиента запланирован асинхронно → выходим из цикла и ждём, когда сработает
            // событие e.Completed.
            if (!listener.AcceptAsync(e))
            {
                continue;
            }

            break;
        }
    }
}