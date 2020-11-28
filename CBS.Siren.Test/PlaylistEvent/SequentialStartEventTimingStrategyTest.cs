using Xunit;
using System;
using System.Collections.Generic;
using CBS.Siren.Time;
using Moq;
using CBS.Siren.Device;

namespace CBS.Siren.Test
{
    public class SequentialStartEventTimingStrategyTest
    {
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void CalculateStartTime_WhenRelatedEventIdIsEmpty_ReportsCurrentTime()
        {
            DateTimeOffset expectedTime = DateTimeOffset.Parse("01/01/2020 15:45:26");
            Mock<ITimeSourceProvider> timeSourceProvider = new Mock<ITimeSourceProvider>();
            timeSourceProvider.Setup(mock => mock.Now).Returns(expectedTime);

            SequentialStartEventTimingStrategy strategy = new SequentialStartEventTimingStrategy()
            {
                Clock = timeSourceProvider.Object
            };

            DateTimeOffset startTime = strategy.CalculateStartTime(null, new TransmissionList(new List<TransmissionListEvent>(), null));

            Assert.Equal(expectedTime, startTime);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void CalculateStartTime_WhenRelatedListIsNull_ReportsCurrentTime()
        {
            DateTimeOffset expectedTime = DateTimeOffset.Parse("01/01/2020 15:45:26");
            Mock<ITimeSourceProvider> timeSourceProvider = new Mock<ITimeSourceProvider>();
            timeSourceProvider.Setup(mock => mock.Now).Returns(expectedTime);

            SequentialStartEventTimingStrategy strategy = new SequentialStartEventTimingStrategy()
            {
                Clock = timeSourceProvider.Object
            };

            DateTimeOffset startTime = strategy.CalculateStartTime(0, null);

            Assert.Equal(expectedTime, startTime);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void CalculateStartTime_WhenRelatedEventIsValidAndPrecedingEventIsNull_ReportsCurrentTime()
        {
            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(), null);
            SequentialStartEventTimingStrategy strategy = new SequentialStartEventTimingStrategy();
            TransmissionListEvent listEvent = new TransmissionListEvent(strategy, new List<IEventFeature>());
            list.Events.Add(listEvent);

            DateTimeOffset target = TimeSource.Now;
            DateTimeOffset startTime = strategy.CalculateStartTime(listEvent.Id, list);

            Assert.Equal(0, target.DifferenceInFrames(startTime));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void CalculateStartTime_WhenEventContainsActualStartTime_ReportsActualStartTime()
        {
            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(), null);
            SequentialStartEventTimingStrategy strategy = new SequentialStartEventTimingStrategy();

            DateTimeOffset target = DateTimeOffset.Parse("01/01/2020 14:30:00");
            TransmissionListEvent listEvent = new TransmissionListEvent(strategy, new List<IEventFeature>())
            {
                Id = 1,
                ExpectedDuration = new TimeSpan(0, 30, 0),
                ExpectedStartTime = target,
                ActualStartTime = target
            };
            list.Events.Add(listEvent);

            DateTimeOffset startTime = strategy.CalculateStartTime(listEvent.Id, list);

            Assert.Equal(0, target.DifferenceInFrames(startTime));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void CalculateStartTime_WhenRelatedEventIsValidAndPrecedingEventIsValid_ReportsStartAfterPreviousEventWithPreroll()
        {
            Mock<ITimeSourceProvider> timeSourceProvider = new Mock<ITimeSourceProvider>();
            timeSourceProvider.Setup(mock => mock.Now).Returns(DateTimeOffset.Parse("01/01/2020 00:00:00"));

            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(), null) 
            { 
                State = TransmissionListState.Stopped 
            };

            TransmissionListEvent precedingEvent = new TransmissionListEvent(null, new List<IEventFeature>())
            {
                Id = 1,
                ExpectedDuration = new TimeSpan(0, 30, 0),
                ExpectedStartTime = DateTimeOffset.Parse("01/01/2020 14:30:00")
            };

            SequentialStartEventTimingStrategy strategy = new SequentialStartEventTimingStrategy()
            {
                Clock = timeSourceProvider.Object
            };
            TransmissionListEvent listEvent = new TransmissionListEvent(strategy, new List<IEventFeature>()) { Id = 2 };

            list.Events.Add(precedingEvent);
            list.Events.Add(listEvent);

            DateTimeOffset target = DateTimeOffset.Parse("01/01/2020 15:00:00");
            DateTimeOffset startTime = strategy.CalculateStartTime(listEvent.Id, list);

            Assert.Equal(target, startTime);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void CalculateStartTime_WhenPrecedingEventHasEndTimeWithinPrerollAndListStopped_ReportsStartAfterPreviousEventWithPreroll()
        {
            Mock<ITimeSourceProvider> timeSourceProvider = new Mock<ITimeSourceProvider>();
            timeSourceProvider.Setup(mock => mock.Now).Returns(DateTimeOffset.Parse("01/01/2020 14:45:00"));

            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(), null)
            {
                State = TransmissionListState.Stopped
            };
            TransmissionListEvent precedingEvent = new TransmissionListEvent(null, new List<IEventFeature>())
            {
                Id = 1,
                ExpectedDuration = new TimeSpan(0, 30, 0),
                ExpectedStartTime = DateTimeOffset.Parse("01/01/2020 14:30:00"),
                ActualEndTime = DateTimeOffset.Parse("01/01/2020 14:45:00")
            };

            SequentialStartEventTimingStrategy strategy = new SequentialStartEventTimingStrategy()
            {
                Clock = timeSourceProvider.Object
            };
            Mock<IDevice> device1 = new Mock<IDevice>();
            device1.Setup(mockDevice => mockDevice.Model).Returns(new DeviceModel() { DeviceProperties = new DeviceProperties() { Preroll = TimeSpan.FromSeconds(20) } });
            Mock<IEventFeature> feature1 = new Mock<IEventFeature>();
            feature1.Setup(mock => mock.Device).Returns(device1.Object);
            List<IEventFeature> features = new List<IEventFeature>() { feature1.Object };
            TransmissionListEvent listEvent = new TransmissionListEvent(strategy, features) { Id = 2 };

            list.Events.Add(precedingEvent);
            list.Events.Add(listEvent);

            DateTimeOffset target = DateTimeOffset.Parse("01/01/2020 14:45:20");
            DateTimeOffset startTime = strategy.CalculateStartTime(listEvent.Id, list);

            Assert.Equal(target, startTime);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void CalculateStartTime_WhenRelatedEventIsValidAndPrecedingEventHasEndTimeAndListRunning_ReportsStartAfterPreviousEvent()
        {
            Mock<ITimeSourceProvider> timeSourceProvider = new Mock<ITimeSourceProvider>();
            timeSourceProvider.Setup(mock => mock.Now).Returns(DateTimeOffset.Parse("01/01/2020 00:00:00"));

            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(), null) 
            {
                State = TransmissionListState.Playing
            };
            TransmissionListEvent precedingEvent = new TransmissionListEvent(null, new List<IEventFeature>())
            {
                Id = 1,
                ExpectedDuration = new TimeSpan(0, 30, 0),
                ExpectedStartTime = DateTimeOffset.Parse("01/01/2020 14:30:00"),
                ActualEndTime = DateTimeOffset.Parse("01/01/2020 14:45:00")
            };

            SequentialStartEventTimingStrategy strategy = new SequentialStartEventTimingStrategy()
            {
                Clock = timeSourceProvider.Object
            };
            TransmissionListEvent listEvent = new TransmissionListEvent(strategy, new List<IEventFeature>()) { Id = 2 };

            list.Events.Add(precedingEvent);
            list.Events.Add(listEvent);

            DateTimeOffset target = DateTimeOffset.Parse("01/01/2020 14:45:00");
            DateTimeOffset startTime = strategy.CalculateStartTime(listEvent.Id, list);

            Assert.Equal(target, startTime);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void CalculateStartTime_WhenCalculatedTimeIsLessThanPrerollInFuture_ReturnsNowPlusLargestPreroll()
        {
            Mock<ITimeSourceProvider> timeSourceProvider = new Mock<ITimeSourceProvider>();
            timeSourceProvider.Setup(mock => mock.Now).Returns(DateTimeOffset.Parse("01/01/2020 14:30:00"));

            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(), null);
            TransmissionListEvent precedingEvent = new TransmissionListEvent(null, new List<IEventFeature>())
            {
                Id = 1,
                ExpectedDuration = new TimeSpan(0, 0, 30),
                ExpectedStartTime = DateTimeOffset.Parse("01/01/2020 14:30:00")
            };

            SequentialStartEventTimingStrategy strategy = new SequentialStartEventTimingStrategy
            {
                Clock = timeSourceProvider.Object
            };

            Mock<IDevice> device1 = new Mock<IDevice>();
            device1.Setup(mockDevice => mockDevice.Model).Returns(new DeviceModel() { DeviceProperties = new DeviceProperties() { Preroll = TimeSpan.FromSeconds(20) } });
            Mock <IEventFeature> feature1 = new Mock<IEventFeature>();
            feature1.Setup(mock => mock.Device).Returns(device1.Object);
            Mock<IDevice> device2 = new Mock<IDevice>();
            device2.Setup(mockDevice => mockDevice.Model).Returns(new DeviceModel() { DeviceProperties = new DeviceProperties() { Preroll = TimeSpan.FromSeconds(40) } });
            Mock<IEventFeature> feature2 = new Mock<IEventFeature>();
            feature2.Setup(mock => mock.Device).Returns(device2.Object);
            List<IEventFeature> features = new List<IEventFeature>() { feature1.Object, feature2.Object };
            TransmissionListEvent listEvent = new TransmissionListEvent(strategy, features) { Id = 2 };

            list.Events.Add(precedingEvent);
            list.Events.Add(listEvent);

            DateTimeOffset target = DateTimeOffset.Parse("01/01/2020 14:30:40");
            DateTimeOffset startTime = strategy.CalculateStartTime(listEvent.Id, list);

            Assert.Equal(target, startTime);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void CalculateStartTime_WhenNoPrecedingEventAndFeatureDevicesHavePreroll_ReportsCurrentTimePlusLargestPreroll()
        {
            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(), null);
            SequentialStartEventTimingStrategy strategy = new SequentialStartEventTimingStrategy();
            Mock<IDevice> device1 = new Mock<IDevice>();
            device1.Setup(mockDevice => mockDevice.Model).Returns(new DeviceModel() { DeviceProperties = new DeviceProperties() { Preroll = TimeSpan.FromSeconds(20) } });
            Mock<IEventFeature> feature1 = new Mock<IEventFeature>();
            feature1.Setup(mock => mock.Device).Returns(device1.Object);
            Mock<IDevice> device2 = new Mock<IDevice>();
            device2.Setup(mockDevice => mockDevice.Model).Returns(new DeviceModel() { DeviceProperties = new DeviceProperties() { Preroll = TimeSpan.FromSeconds(40) } });
            Mock<IEventFeature> feature2 = new Mock<IEventFeature>();
            feature2.Setup(mock => mock.Device).Returns(device2.Object);
            List<IEventFeature> features = new List<IEventFeature>() { feature1.Object, feature2.Object };

            TransmissionListEvent listEvent = new TransmissionListEvent(strategy, features);
            list.Events.Add(listEvent);

            DateTimeOffset target = TimeSource.Now + TimeSpan.FromSeconds(40);
            DateTimeOffset startTime = strategy.CalculateStartTime(listEvent.Id, list);

            Assert.Equal(0, target.DifferenceInFrames(startTime));
        }
    }
}