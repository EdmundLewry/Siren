using System;

namespace PBS.Siren
{
    /*
    A PlayoutListEvent is a lightweight container for the data required by a specific device in order for
    that device to be able to enact the event at the correct time. 
    */
    class PlayoutListEvent
    {
        //We may want more human readable identifiers
        public Guid Id { get; set; }

        public PlayoutListEventState EventState { get; set; }

        public PlayoutListEvent()
        {
            
        }
    }
}