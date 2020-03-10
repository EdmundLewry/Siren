using System.Collections.Generic;

namespace PBS.Siren
{
    public interface IPlaylist
    {
        List<TransmissionEvent> Events { get; }
    }
}