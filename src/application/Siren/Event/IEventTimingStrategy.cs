using System;

namespace PBS.Siren
{
    /*
    An Event Timing Strategy is the logic for an individual Transmission Event which is used to generate
    the Start Time of that event. 

    The timing of the event could be relative to another event (Sequential being the simplest example of this)
    or could be a fixed point in time. 
    */
    interface IEventTimingStrategy
    {
        
    }
}