using Course.Contracts.Contracts.Serialize;

namespace Course.Contracts;

public static class ResponseHelper
{
    public static Envelope<Fault> GetFaultEnvelope(Fault fault)
    {
        return new Envelope<Fault>
        {
            Body = new Body<Fault>
            {
                Content = fault
            }
        };
    }
}