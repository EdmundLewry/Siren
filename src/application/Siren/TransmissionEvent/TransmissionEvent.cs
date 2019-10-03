using System;

namespace PBS.Siren
{
    /*
    A transmission event is an item in a Transmission List that defines what should be played out,
    for how long, when it should start, where in the channel layout it should be displayed,
    and validates that that data makes sense against the source being represented.
     */
    public class TransmissionEvent
    {
        public ISourceStrategy SourceStrategy { get; set; }
        public IPlayoutStrategy PlayoutStrategy { get; set; }
        public IEventTimingStrategy EventTimingStrategy { get; set; }

        //This should be in timecode
        public int Duration { get; set; } //Currently in frames (assuming 25FPS)
        //Should be in timecode
        public DateTime StartTime { get; set; }
        public bool IsValid { get; set; }
        public String ValidationStatus { get; set; }

        //We may want more human readable identifiers
        public Guid Id { get; set; }
        
        public TransmissionEvent(ISourceStrategy source, IPlayoutStrategy playout, IEventTimingStrategy timingStrategy)
        {
            SourceStrategy = source;
            PlayoutStrategy = playout;
            EventTimingStrategy = timingStrategy;
            Id = Guid.NewGuid();
        }

        public override String ToString()
        {
            return base.ToString() + " Id: " + Id.ToString() + " StartTime: " + StartTime;
        }
    }
}
