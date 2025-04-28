using Course.Contracts.Contracts.Responses;
using Course.Contracts.Contracts.Serialize;

namespace Course.Contracts.Helpers;

public static class ResponseHelper
{
    // public static Envelope<FaultResponse> GetFaultEnvelope(FaultResponse faultResponse)
    // {
    //     return new Envelope<FaultResponse>
    //     {
    //         Body = new Body<FaultResponse>
    //         {
    //             Content = faultResponse
    //         }
    //     };
    // }
    
    public static Envelope<T> GetEnvelope<T>(T obj) where T : class, IResponse
    {
        return new Envelope<T>
        {
            Body = new Body<T>
            {
                Content = obj
            }
        };
    }
}