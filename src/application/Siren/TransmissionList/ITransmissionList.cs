using System.Collections.Generic;

namespace PBS.Siren
{
    public interface ITransmissionList
    {
        List<TransmissionEvent> Events { get; }
    }
}