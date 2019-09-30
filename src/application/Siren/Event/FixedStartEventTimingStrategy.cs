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
    }
}