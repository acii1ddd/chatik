namespace Course.Contracts.Contracts.Responses.ForToday;

public class GetWeatherForTodayResponse : IResponse
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string TimeZone { get; init; } = string.Empty;
    
    // время получения последних данных о погоде (каждые 15 мин)
    public string Time { get; init; } = string.Empty;
    
    // интервал обновления данных на api (15 мин)
    public int Interval { get; init; }
    public double Temperature { get; init; }
    public double WindSpeed { get; init; }
    
    // направление ветра в градусах
    public double WindDirection { get; init; }
    
    // 0 - сейчас ночь (солнце зашло), 1 — день
    public int IsDay { get; init; }
}
