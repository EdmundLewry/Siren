using System;

namespace PBS.Siren
{
    public class FixedStartEventTimingStrategy : IEventTimingStrategy
    {
        public DateTime TargetStartTime { get; }
        public FixedStartEventTimingStrategy(DateTime startTime)
        {
            TargetStartTime = startTime;
        }

        public DateTime CalculateStartTime()
        {
            //Fixed time is really easy! We just send back what we got in
            return TargetStartTime;
        }
    }
}