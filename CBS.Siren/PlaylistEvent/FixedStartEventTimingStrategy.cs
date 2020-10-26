using System;
using System.Diagnostics.CodeAnalysis;

namespace CBS.Siren
{
    public class FixedStartEventTimingStrategy : IEventTimingStrategy
    {
        public string StrategyType => "fixed";
        public DateTimeOffset? TargetStartTime { get; }

        public FixedStartEventTimingStrategy(DateTimeOffset startTime)
        {
            TargetStartTime = startTime;
        }
        
        public FixedStartEventTimingStrategy(IEventTimingStrategy other)
        {
            if(other is FixedStartEventTimingStrategy fixedStrategy)
            {
                TargetStartTime = fixedStrategy.TargetStartTime;
                return;
            }

            throw new ArgumentException("Failed to construct timing strategy. Given strategy was not the same type", "other");
        }

        public DateTimeOffset CalculateStartTime(int? eventId, TransmissionList list)
        {
            //Fixed time is really easy! We just send back what we got in
            return TargetStartTime.Value;
        }

        public override string ToString()
        {
            return "FixedStartEventTimingStrategy:" +
            $"TargetStartTime: {TargetStartTime}";
        }

        public virtual bool Equals([AllowNull] IEventTimingStrategy other)
        {
            return other is FixedStartEventTimingStrategy fixedStrategy &&
                TargetStartTime == fixedStrategy.TargetStartTime;
        }
    }
}