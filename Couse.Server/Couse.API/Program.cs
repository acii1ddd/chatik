using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;

class Program
{
    static async Task Main(string[] args)
    {
        // var listener = new HttpListener();
        // listener.Prefixes.Add("http://localhost:5005/soap/");
        // listener.Start();
        // Console.WriteLine("Listening for requests...");
        //
        // while (true)
        // {
        //     var context = listener.GetContext();
        //     using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
        //     {
        //         Console.WriteLine("Requesting to {0}", context.Request.RawUrl);
        //         
        //         var requestBody = reader.ReadToEnd();
        //         
        //         var responseXml = ProcessRequest(requestBody);
        //         var bufferResponse = Encoding.UTF8.GetBytes(responseXml);
        //         
        //         context.Response.ContentType = "text/xml;charset=utf-8";
        //         using (var outputStream = context.Response.OutputStream)
        //         {
        //             outputStream.Write(bufferResponse, 0, bufferResponse.Length);
        //         }
        //     }
        // }
        
        const int port = 5005;
        var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
        serverSocket.Listen(10);
        Console.WriteLine("Listening for requests on port {0}...", port);

        while (true)
        {
            // принимает входящее подключение
            var clientSocket = await serverSocket.AcceptAsync();
            Console.WriteLine("Requesting...");
            
            var buffer = new byte[1024];
            var bytesReceived = clientSocket.Receive(buffer);
            if (bytesReceived == 0)
            {
                Console.WriteLine("Received zero bytes");
            }

            var request = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
            Console.WriteLine("Request: {0}", request);

            var response = ProcessRequest(request);
            
            var httpResponse =
                "HTTP/1.1 200 OK\r\n" +
                "Content-Type: text/xml; charset=utf-8\r\n" +
                $"Content-Length: {Encoding.UTF8.GetByteCount(response)}\r\n" +
                "\r\n" +
                response;

            Console.WriteLine("Response {0}", httpResponse);
            
            clientSocket.Send(Encoding.UTF8.GetBytes(httpResponse));
            clientSocket.Close();
        }
    }

    private static string ProcessRequest(string requestBody)
    {
        if (requestBody == string.Empty)
        {
            Console.WriteLine("Request body is empty.");
            //return GetErrorResponse("Empty request body.");
        }
        
        
        // парсим request
        return CreateResponse("Запрос обработан");
    }
    
    private static string CreateResponse(string message)
    {
        return $"""
                <?xml version="1.0"?>
                        <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                            <soap:Body>
                                <Response>{message}</Response>
                            </soap:Body>
                        </soap:Envelope>
                """;
    }
    
    private static string GetErrorResponse(string errorMessage)
    {
        // TODO
        return new string("213");
    }
}