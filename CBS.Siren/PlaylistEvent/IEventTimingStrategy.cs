using System;

namespace CBS.Siren
{
    /*
    An Event Timing Strategy is the logic for an individual Transmission Event which is used to generate
    the Start Time of that event. 

    The timing of the event could be relative to another event (Sequential being the simplest example of this)
    or could be a fixed point in time. 
    */
    public interface IEventTimingStrategy : IEquatable<IEventTimingStrategy>
    {
        string StrategyType { get; }
        /* Change this so that it receives id of the event being calculated on, and the list */
        DateTime CalculateStartTime(Guid eventId, TransmissionList list);
        string ToString();
    }
}