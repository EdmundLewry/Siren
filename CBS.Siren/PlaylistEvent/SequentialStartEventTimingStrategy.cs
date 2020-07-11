using System;
using System.Diagnostics.CodeAnalysis;

namespace CBS.Siren
{
    public class SequentialStartEventTimingStrategy : IEventTimingStrategy
    {
        public string StrategyType => "sequential";
        
        public SequentialStartEventTimingStrategy() {}
        public SequentialStartEventTimingStrategy(IEventTimingStrategy other)
        {
            if(other is SequentialStartEventTimingStrategy)
            {
                return;
            }

            throw new ArgumentException("Failed to construct timing strategy. Given strategy was not the same type", "other");
        }

        public DateTime CalculateStartTime(int? eventId, TransmissionList list)
        {
            if(!eventId.HasValue || list == null)
            {
                return DateTime.Now;
            }

            //This is not very efficient...
            TransmissionListEvent precedingEvent = null;
            foreach(TransmissionListEvent listEvent in list.Events)
            {
                if(listEvent.Id == eventId)
                {
                    break;
                }

                precedingEvent = listEvent;
            }

            if(precedingEvent == null)
            {
                return DateTime.Now;
            }

            return precedingEvent.ExpectedStartTime + precedingEvent.ExpectedDuration;
        }

        public override string ToString()
        {
            return "SequentialStartEventTimingStrategy";
        }

        public virtual bool Equals([AllowNull] IEventTimingStrategy other)
        {
            return other is SequentialStartEventTimingStrategy;
        }
    }
}