using System;
using System.Collections.Generic;

/*
A transmission list is an ordered series of transmission events
which are going to be played out on a channel. The transmission list
is just responsible for maintaining the list of events and validating
that this list makes sense to play out.

TODO:
- Should this be an immutable object?
 */
namespace CBS.Siren
{
    public class Playlist : IPlaylist
    {
        //Holds a collection of events
        //Validates the collection of events in relation to each other when something changes
        //Supports adding, removing, reordering events
        //Supports getting events
        public List<PlaylistEvent> Events { get; }
        
        public Playlist(List<PlaylistEvent> listEvents)
        {
            Events = listEvents;
        }
    }
}