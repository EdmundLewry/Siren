using Xunit;

using System;
using System.Collections.Generic;

using CBS.Siren.Time;

namespace CBS.Siren.Test
{
    public class SequentialStartEventTimingStrategyTest
    {
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void CalculateStartTime_WhenRelatedEventIdIsEmpty_ReportsCurrentTime()
        {
            SequentialStartEventTimingStrategy strategy = new SequentialStartEventTimingStrategy();

            DateTime target = DateTime.Now;
            DateTime startTime = strategy.CalculateStartTime(null, new TransmissionList(new List<TransmissionListEvent>(), null));

            //Using difference in frames to account for potential millisecond difference in DateTime.Now
            //rather than writing a Time abstraction
            Assert.True(target.DifferenceInFrames(startTime)==0);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void CalculateStartTime_WhenRelatedListIsNull_ReportsCurrentTime()
        {
            SequentialStartEventTimingStrategy strategy = new SequentialStartEventTimingStrategy();

            DateTime target = DateTime.Now;
            DateTime startTime = strategy.CalculateStartTime(0, null);

            //Using difference in frames to account for potential millisecond difference in DateTime.Now
            //rather than writing a Time abstraction
            Assert.True(target.DifferenceInFrames(startTime)==0);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void CalculateStartTime_WhenRelatedEventIsValidAndPrecedingEventIsNull_ReportsCurrentTime()
        {
            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(), null);
            SequentialStartEventTimingStrategy strategy = new SequentialStartEventTimingStrategy();
            TransmissionListEvent listEvent = new TransmissionListEvent(strategy, new List<IEventFeature>());
            list.Events.Add(listEvent);

            DateTime target = DateTime.Now;
            DateTime startTime = strategy.CalculateStartTime(listEvent.Id, list);

            Assert.True(target.DifferenceInFrames(startTime)==0);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void CalculateStartTime_WhenRelatedEventIsValidAndPrecedingEventIsValid_ReportsStartAfterPreviousEvent()
        {
            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(), null);
            TransmissionListEvent precedingEvent = new TransmissionListEvent(null, new List<IEventFeature>());
            precedingEvent.ExpectedDuration = new TimeSpan(0,30,0);
            precedingEvent.ExpectedStartTime = DateTime.Parse("01/01/2020 14:30:00");

            SequentialStartEventTimingStrategy strategy = new SequentialStartEventTimingStrategy();
            TransmissionListEvent listEvent = new TransmissionListEvent(strategy, new List<IEventFeature>());
            list.Events.Add(precedingEvent);
            list.Events.Add(listEvent);

            DateTime target = DateTime.Parse("01/01/2020 15:00:00");
            DateTime startTime = strategy.CalculateStartTime(listEvent.Id, list);

            Assert.Equal(target, startTime);
        }
    }
}