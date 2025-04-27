// using System.Collections.Concurrent;
// using System.Net;
// using System.Net.Sockets;
// using System.Text;
// using System.Xml;
//
// namespace Couse.Server;
//
// class Program
// {
//     private static ConcurrentDictionary<string, Socket> _clients = new();
//
//     private static async Task Main(string[] args)
//     {
//         await StartServer();
//     }
//
//     private static async Task StartServer()
//     {
//         var listener = new TcpListener(IPAddress.Any, port);
//         
//         const int port = 5005;
//         var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//         
//         serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
//         serverSocket.Listen(10);
//         
//         Console.WriteLine("Listening for requests on port {0}...", port);
//
//         while (true)
//         {
//             try
//             {
//                 // принимаем входящее подключение (клиентский сокет)
//                 var clientSocket = await serverSocket.AcceptAsync();
//                 Console.WriteLine("Requesting...");
//             
//                 // явно не ждем
//                 _ = Task.Run(() => HandleRequest(clientSocket));
//             }
//             catch (Exception e)
//             {
//                 Console.WriteLine(e);
//             }
//         }
//     }
//
//     private static async Task HandleRequest(Socket clientSocket)
//     {
//         var buffer = new byte[1024];
//         var bytesReceived = await clientSocket.ReceiveAsync(buffer);
//         if (bytesReceived == 0)
//         {
//             Console.WriteLine("Received zero bytes");
//         }
//
//         var request = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
//         Console.WriteLine("Request: {0}", request);
//
//         // HARDCODE
//         if (Guid.TryParse(request, out var clientId))
//         {
//             _clients.TryAdd(clientId.ToString(), clientSocket);
//             return;
//         }
//         // HARDCODE
//
//         //var payload = ParseSoapRequest(request);
//         
//         //_clients.TryAdd(payload.senderId, clientSocket);
//             
//         var httpResponse =
//             "HTTP/1.1 200 OK\r\n" +
//             "Content-Type: text/xml; charset=utf-8\r\n" +
//             $"Content-Length: {Encoding.UTF8.GetByteCount(CreateResponse("123"))}\r\n" +
//             "\r\n" +
//             CreateResponse("123");
//
//         Console.WriteLine("Пользователи: ");
//         foreach (var id in _clients.Keys) {
//             Console.WriteLine("{0}", id);
//         }
//         
//         Console.WriteLine("\nResponse {0}", httpResponse);
//             
//         // отправляем заданному клиенту
    //         //_clients[payload.receiverId].Send(Encoding.UTF8.GetBytes(httpResponse));
//         _clients[].Send(Encoding.UTF8.GetBytes(httpResponse));
//         
//         // clientSocket.Close();
//     }
//     
//     private static (string senderId, string receiverId, string message) ParseSoapRequest(string request)
//     {
//         if (request == string.Empty)
//         {
//             Console.WriteLine("Request body is empty.");
//             //return GetErrorResponse("Empty request body.");
//         }
//         
//         // парсинг xml
//         var xmlDoc = new XmlDocument();
//         xmlDoc.LoadXml(request);
//         
//         var senderIdNode = xmlDoc.SelectSingleNode("//soap:Header/SenderId", GetNamespaceManager(xmlDoc));
//         var senderId = senderIdNode?.InnerText
//             ?? throw new Exception("Sender ID is missing.");
//         
//         var receiverIdNode = xmlDoc.SelectSingleNode("//soap:Header/ReceiverId", GetNamespaceManager(xmlDoc));
//         var receiverId = receiverIdNode?.InnerText
//                        ?? throw new Exception("Receiver ID is missing.");
//         
//         var messageNode = xmlDoc.SelectSingleNode("//soap:Body/SendMessage/Message", GetNamespaceManager(xmlDoc));
//         var message = messageNode?.InnerText
//             ?? throw new Exception("Message is missing.");
//         
//         return (senderId, receiverId, message);
//     }
//
//     private static XmlNamespaceManager GetNamespaceManager(XmlDocument xmlDoc)
//     {
//         var manager = new XmlNamespaceManager(xmlDoc.NameTable);
//         manager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
//         return manager;
//     }
//
//     private static string CreateResponse(string message)
//     {
//         return $"""
//                 <?xml version="1.0"?>
//                         <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
//                             <soap:Body>
//                                 <Response>{message}</Response>
//                             </soap:Body>
//                         </soap:Envelope>
//                 """;
//     }
//     
//     private static string GetErrorResponse(string errorMessage)
//     {
//         // TODO
//         return new string("213");
//     }
// }


using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Xml.Linq;

namespace Couse.API;

public static class Server
{
    private const int Port = 5000;
    private const int BufferSize = 1024;

    private static void Main()
    {
        var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Any, Port));
        listener.Listen(100);

        Console.WriteLine($"Сервер слушает на порту {Port}...");
    
        StartAccept(listener);
        
        Console.ReadLine();
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

            var readEventArg = new SocketAsyncEventArgs();
            readEventArg.SetBuffer(new byte[BufferSize], 0, BufferSize);
            readEventArg.UserToken = client;
            readEventArg.Completed += IO_Completed!;

            // true - асинхронная операция -> сработает Completed
            // false - синхронная операция
            if (!client.ReceiveAsync(readEventArg))
            {
                ProcessReceive(readEventArg);
            }

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

    // для всех завершённых I/O-операций на сокетах, связанных с SocketAsyncEventArgs
    private static void IO_Completed(object sender, SocketAsyncEventArgs e)
    {
        switch (e.LastOperation)
        {
            case SocketAsyncOperation.Receive:
                ProcessReceive(e);
                break;
            case SocketAsyncOperation.Send:
                ProcessSend(e);
                break;
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
                CloseClient(client);
                return;
            }
            var request = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);
            
            // логика запроса на weather api
            
                
            
            
            
            // Console.WriteLine($"Получено: {request}");
            //
            // string response;
            //
            // // Обрабатываем команду
            // if (request.StartsWith("GET_DOCTORS"))
            // {
            //     var branchName = request.Substring("GET_DOCTORS".Length).Trim();
            //     response = GetDoctorsXml(branchName);  // Формируем ответ
            // }
            // else
            // {
            //     response = BuildXml("Неизвестная команда");
            // }
            
            const string testResponse = "test-response";
            Console.WriteLine("\nResponse text: {0}", testResponse);
            
            var responseBytes = Encoding.UTF8.GetBytes(testResponse);
            e.SetBuffer(responseBytes, 0, responseBytes.Length);

            if (!client.SendAsync(e))
            {
                
                ProcessSend(e);
            }
        }
        else
        {
            CloseClient(client);
        }
    }

    private static string GetDoctorsXml(string branchName)
    {
        // Здесь ты можешь использовать свою логику для получения врачей
        var doc = new XDocument(
            new XElement("Response",
                new XElement("Branch", branchName),
                new XElement("Doctors",
                    new XElement("Doctor", new XElement("Name", "Доктор 1"), new XElement("Specialization", "Терапевт")),
                    new XElement("Doctor", new XElement("Name", "Доктор 2"), new XElement("Specialization", "Хирург"))
                ),
                new XElement("Time", DateTime.UtcNow.ToString("o"))
            )
        );
        return doc.ToString(SaveOptions.DisableFormatting);
    }

    private static void ProcessSend(SocketAsyncEventArgs e)
    {
        if (e.UserToken is null) return;
        var client = (Socket)e.UserToken;

        Console.WriteLine($"[{DateTime.UtcNow:O}] Response sent to {client.RemoteEndPoint}, {e.BytesTransferred} bytes.");
        
        // Готовим новый буфер на чтение
        e.SetBuffer(new byte[BufferSize], 0, BufferSize);
        
        if (!client.ReceiveAsync(e))
        {
            ProcessReceive(e);
        }
    }

    private static string BuildXml(string input)
    {
        var doc = new XDocument(
            new XElement("Response",
                new XElement("Message", $"Ты написал: {input}"),
                new XElement("Time", DateTime.UtcNow.ToString("o"))
            )
        );
        return doc.ToString(SaveOptions.DisableFormatting);
    }

    private static void CloseClient(Socket client)
    {
        Console.WriteLine("\nКлиент отключён.");
        try { client.Shutdown(SocketShutdown.Both); } catch { }
        client.Close();
    }
}