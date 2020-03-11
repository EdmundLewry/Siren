using System;

namespace CBS.Siren
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

        public string BuildEventData()
        {
            throw new NotImplementedException();
        }
    }
}