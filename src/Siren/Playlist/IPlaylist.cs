using System.Collections.Generic;

namespace PBS.Siren
{
    public interface IPlaylist
    {
        List<PlaylistEvent> Events { get; }
    }
}