namespace WeatherPipe.Client;

internal static class ErrorHandler
{
    internal static bool IsFaultResponse(string xmlResponseStr)
    {
        return xmlResponseStr.Contains("<Fault>");
    }
}