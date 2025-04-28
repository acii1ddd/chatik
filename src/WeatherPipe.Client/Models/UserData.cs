namespace WeatherPipe.Client.Models;

public class UserData
{
    public string Country { get; init; } = string.Empty;
    
    public string City { get; init; } = string.Empty;
    
    public double Lat { get; init; }
    
    public double Lon { get; init; }
    
    public string Timezone { get; init; } = string.Empty;
}