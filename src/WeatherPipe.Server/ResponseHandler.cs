using System.Net.Sockets;
using System.Text;
using Course.Contracts;
using Course.Contracts.Contracts.Responses;
using Course.Contracts.Contracts.Responses.ForCity;
using Course.Contracts.Contracts.Responses.ForToday;
using Course.Contracts.Contracts.Serialize;
using Course.Contracts.Helpers;

namespace Couse.API;

internal static class ResponseHandler
{
    internal static void HandleResponse(Socket client, SocketAsyncEventArgs e, Envelope<IResponse> weatherResponse)
    {
        switch (weatherResponse.Body.Content)
        {
            case GetWeatherForCityResponse weatherContent:
            {
                var typedEnvelope = new Envelope<GetWeatherForCityResponse>
                {
                    Body = new Body<GetWeatherForCityResponse>
                    {
                        Content = weatherContent
                    }
                };
                SendXmlResponse(client, e, typedEnvelope);
                break;
            }
            case GetWeatherForTodayResponse weatherContent:
            {
                var typedEnvelope = new Envelope<GetWeatherForTodayResponse>
                {
                    Body = new Body<GetWeatherForTodayResponse>
                    {
                        Content = weatherContent
                    }
                };
                SendXmlResponse(client, e, typedEnvelope);
                break;
            }
            case FaultResponse faultContent:
            {
                var typedEnvelope = new Envelope<FaultResponse>
                {
                    Body = new Body<FaultResponse>
                    {
                        Content = faultContent
                    }
                };
                SendXmlResponse(client, e, typedEnvelope);
                break;
            }
        }
    }

    private static void SendXmlResponse<T>(Socket client, SocketAsyncEventArgs e, Envelope<T> xmlResponseEnvelope) where T : class
    {
        var xmlResponseStr = XmlHelper.SerializeToXml(xmlResponseEnvelope);
        Console.WriteLine("\n\nResponse text: {0}", xmlResponseStr);
     
        var responseBytes = Encoding.UTF8.GetBytes(xmlResponseStr);
        e.SetBuffer(responseBytes, 0, responseBytes.Length);

        if (!client.SendAsync(e))
        {
            ProcessSend(e);
        }
    }

    private static void ProcessSend(SocketAsyncEventArgs e)
    {
        if (e.UserToken is null) return;
        var client = (Socket)e.UserToken;

        Console.WriteLine($"[{DateTime.UtcNow:O}] Response sent to {client.RemoteEndPoint}, {e.BytesTransferred} bytes.");
        
        // Готовим новый буфер на чтение
        // e.SetBuffer(new byte[BufferSize], 0, BufferSize);
        // if (!client.ReceiveAsync(e))
        // {
        //     ProcessReceive(e);
        // }
        
        // закрываем соединение — больше не ждём новых запросов по этому сокету
        // client.Shutdown(SocketShutdown.Both);
        // client.Close();

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