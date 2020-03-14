using System.Collections.Generic;

namespace CBS.Siren
{
    public interface IPlaylist
    {
        List<PlaylistEvent> Events { get; }
    }
}