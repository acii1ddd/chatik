using System.Text.Json;
using WeatherPipe.Client.Models;

namespace WeatherPipe.Client.Utils;

public static class Location
{
    public static async Task<UserData> GetUserData()
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync("http://ip-api.com/json");
        
        var userData = JsonDocument.Parse(response);
        var userDataRoot = userData.RootElement;
        
        var country = userDataRoot.GetProperty("country").GetString() ?? string.Empty;
        var city = userDataRoot.GetProperty("city").GetString() ?? string.Empty;
        var lat = userDataRoot.GetProperty("lat").GetDouble();
        var lon = userDataRoot.GetProperty("lon").GetDouble();
        var timezone = userDataRoot.GetProperty("timezone").GetString() ?? string.Empty;

        return new UserData
        {
            Country = country,
            City = city,
            Lat = lat,
            Lon = lon,
            Timezone = timezone
        };
    }
}