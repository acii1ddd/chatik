using System.Net.Sockets;

namespace Course.Contracts;

public static class ConnectionManager
{
    public static void CloseClient(Socket client)
    {
        if (client.Connected)
        {
            client.Shutdown(SocketShutdown.Both);
        }
        client.Close();
    }
}