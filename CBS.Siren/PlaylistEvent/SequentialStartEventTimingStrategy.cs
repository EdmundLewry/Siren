using CBS.Siren.Time;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CBS.Siren
{
    public class SequentialStartEventTimingStrategy : IEventTimingStrategy
    {
        public string StrategyType => "sequential";

        public DateTimeOffset? TargetStartTime => null;
        public ITimeSourceProvider Clock { get; set; } = TimeSource.TimeProvider;

        public SequentialStartEventTimingStrategy() {}
        public SequentialStartEventTimingStrategy(IEventTimingStrategy other)
        {
            if(other is SequentialStartEventTimingStrategy)
            {
                return;
            }

            throw new ArgumentException("Failed to construct timing strategy. Given strategy was not the same type", "other");
        }

        public DateTimeOffset CalculateStartTime(int? eventId, TransmissionList list)
        {
            if(!eventId.HasValue || list == null)
            {
                return Clock.Now;
            }

            //This is not very efficient...
            TransmissionListEvent precedingEvent = null;
            TransmissionListEvent relatedEvent = null;
            foreach(TransmissionListEvent listEvent in list.Events)
            {
                if(listEvent.Id == eventId)
                {
                    relatedEvent = listEvent;
                    break;
                }

                precedingEvent = listEvent;
            }

            if(relatedEvent?.ActualStartTime != null)
            {
                return relatedEvent.ActualStartTime.Value;
            }

            bool scheduleWithPreroll = true;
            DateTimeOffset calculatedStartTime = Clock.Now;
            if (precedingEvent != null)
            {
                calculatedStartTime = precedingEvent.ActualEndTime ?? precedingEvent.ExpectedStartTime + precedingEvent.ExpectedDuration;
                scheduleWithPreroll = ShouldScheduleWithPreroll(list, precedingEvent);
            }

            TimeSpan largestPreroll = CalculateLargestDevicePreroll(relatedEvent);
            if(scheduleWithPreroll && StartIsWithinPreroll(calculatedStartTime, largestPreroll))
            {
                calculatedStartTime = Clock.Now + largestPreroll;
            }

            return calculatedStartTime;
        }

        private bool ShouldScheduleWithPreroll(TransmissionList list, TransmissionListEvent precedingEvent)
        {
            //If the preceding event has an actual endtime, and we're running, then we can't use preroll as we need to
            //try to start immediately.
            return list.State != TransmissionListState.Playing || !precedingEvent.ActualEndTime.HasValue;
        }

        private bool StartIsWithinPreroll(DateTimeOffset calculatedStartTime, TimeSpan largestPreroll)
        {
            return (calculatedStartTime - Clock.Now) < largestPreroll;
        }

        private TimeSpan CalculateLargestDevicePreroll(TransmissionListEvent relatedEvent)
        {
            TimeSpan? largestPreroll = relatedEvent.EventFeatures.Max((feature) => feature?.Device?.Model.DeviceProperties.Preroll);
            return largestPreroll ?? TimeSpan.Zero;
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