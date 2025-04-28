using Course.Contracts.Contracts.Requests;
using Course.Contracts.Contracts.Responses;
using Course.Contracts.Contracts.Serialize;
using Course.Contracts.Helpers;
using Couse.API.Services;

namespace Couse.API;

public static class RequestProcessor
{
    internal static Envelope<IResponse> ProcessRequest(string requestXmlString)
    {
        if (string.IsNullOrEmpty(requestXmlString))
            return ResponseHelper.GetEnvelope<IResponse>(new FaultResponse
            {
                Code = "Client",
                Message = "Получен пустой запрос!"
            }
        );
        
        if (requestXmlString.Contains("<RequestType>GetWeatherForCity</RequestType>"))
        {
            var envelopeRequest = XmlHelper.XmlDeserialize<GetWeatherForCityRequest>(requestXmlString);
            var city = envelopeRequest?.Body.Content.City
                ?? "";
            
            Console.WriteLine($"Город = {city}");
            var result = WeatherService.GetForecastForCity(city);
            return ResponseHelper.GetEnvelope(result);
        }

        if (requestXmlString.Contains("<RequestType>GetWeatherForToday</RequestType>"))
        {
            var envelopeRequest = XmlHelper.XmlDeserialize<GetWeatherForTodayRequest>(requestXmlString);
            var getWeatherForTodayRequest = envelopeRequest?.Body.Content;
            Console.WriteLine($"Широта: {getWeatherForTodayRequest?.Latitude}, Долгота: {getWeatherForTodayRequest?.Longitude}" );
            
            if (getWeatherForTodayRequest is null)
            {
                return ResponseHelper.GetEnvelope<IResponse>(new FaultResponse
                {
                    Code = "Client",
                    Message = "В запросе отсутствуют координаты!"
                });
            }
            
            var result = WeatherService.GetForecastForToday(
                getWeatherForTodayRequest.Latitude, 
                getWeatherForTodayRequest.Longitude
            );
            
            return ResponseHelper.GetEnvelope(result);
        }
        
        if (requestXmlString.Contains("<RequestType><RequestType>"))
        {
            //var envelopeRequest = XmlHelper.XmlDeserialize<GetWeatherForCityRequest>(requestXmlString);
            //var getWeatherForCityRequest = envelopeRequest?.Body.Content;
            
            return ResponseHelper.GetEnvelope<IResponse>(
                new FaultResponse
                {
                    Code = "Server",
                    Message = "Не реализован"
                }
            );
        }

        return ResponseHelper.GetEnvelope<IResponse>(new FaultResponse
        {
            Code = "Client",
            Message = "Нет заданного действия!"
        });
    }
}