namespace Couse.API;

internal static class Program
{
    private static void Main()
    {
        SocketServer.Start();
        Console.ReadLine();
    }
}