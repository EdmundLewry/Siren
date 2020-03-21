using System;
using System.Diagnostics.CodeAnalysis;

namespace CBS.Siren
{
    public class FixedStartEventTimingStrategy : IEventTimingStrategy
    {
        public string StrategyType => "fixed";
        public DateTime TargetStartTime { get; }

        public FixedStartEventTimingStrategy(DateTime startTime)
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

        public DateTime CalculateStartTime()
        {
            //Fixed time is really easy! We just send back what we got in
            return TargetStartTime;
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