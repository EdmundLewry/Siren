﻿using CBS.Siren.Device;
using CBS.Siren.Time;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace CBS.Siren.Test
{
    public class SimpleSchedulerUnitTests
    {
        private readonly Mock<IDevice> mockDevice1 = new Mock<IDevice>();
        private readonly Mock<IDevice> mockDevice2 = new Mock<IDevice>();
        private readonly Mock<IDevice> mockDevice3 = new Mock<IDevice>();
        
        private TransmissionListEvent event1;
        private TransmissionListEvent event2;
        private TransmissionListEvent event3;
        private TransmissionListEvent event4;
        private readonly TransmissionList transmissionList;

        public SimpleSchedulerUnitTests()
        {
            transmissionList = GenerateTransmissionList();
        }

        public TransmissionList GenerateTransmissionList()
        {
            var mockEventTimingStrategy = new Mock<IEventTimingStrategy>();
            mockEventTimingStrategy.Setup(mock => mock.CalculateStartTime()).Returns(DateTime.Parse("01/01/2020 14:30:00"));

            var mockFeature = new Mock<IEventFeature>();
            MediaInstance mediaInstance = new MediaInstance("test1");
            mockFeature.Setup(mock => mock.SourceStrategy).Returns(new MediaSourceStrategy(mediaInstance, 0, 750));
            mockFeature.Setup(mock => mock.CalculateDuration()).Returns(750);
            mockFeature.Setup(mock => mock.Device).Returns(mockDevice1.Object);
            List<IEventFeature> eventFeatures = new List<IEventFeature>() { mockFeature.Object };

            var mockFeature2 = new Mock<IEventFeature>();
            mockFeature2.Setup(mock => mock.Device).Returns(mockDevice2.Object);
            
            var mockFeature3 = new Mock<IEventFeature>();
            mockFeature3.Setup(mock => mock.Device).Returns(mockDevice3.Object);

            event1 = new TransmissionListEvent(mockEventTimingStrategy.Object, eventFeatures);
            event2 = new TransmissionListEvent(mockEventTimingStrategy.Object, eventFeatures);
            event3 = new TransmissionListEvent(mockEventTimingStrategy.Object, new List<IEventFeature>() { mockFeature2.Object, mockFeature3.Object});
            event4 = new TransmissionListEvent(mockEventTimingStrategy.Object, eventFeatures);

            return new TransmissionList(new List<TransmissionListEvent>() {event1, event2, event3, event4}, null);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTransmissionList_ShouldCreateOneDeviceListForEachDevice()
        {
            var mockDeviceEventFactory = new Mock<IDeviceListEventFactory>();
            mockDeviceEventFactory.Setup(mockDeviceEventFactory => mockDeviceEventFactory.CreateDeviceListEvent(It.IsAny<string>(), It.IsAny<Guid>()))
                                  .Returns((string s, Guid id) => new DeviceListEvent(s, id));

            SimpleScheduler simpleChannelScheduler = new SimpleScheduler();
            Dictionary<IDevice, DeviceList> lists = simpleChannelScheduler.ScheduleTransmissionList(transmissionList, mockDeviceEventFactory.Object);

            Assert.True(lists.ContainsKey(mockDevice1.Object));
            Assert.True(lists.ContainsKey(mockDevice2.Object));
            Assert.True(lists.ContainsKey(mockDevice3.Object));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTransmissionList_ShouldCreateListsOfOnlyEventsRelatedToOneDevice()
        {
            var mockDeviceEventFactory = new Mock<IDeviceListEventFactory>();
            mockDeviceEventFactory.Setup(mockDeviceEventFactory => mockDeviceEventFactory.CreateDeviceListEvent(It.IsAny<string>(), It.IsAny<Guid>()))
                                  .Returns((string s, Guid id) => new DeviceListEvent(s, id));

            SimpleScheduler simpleChannelScheduler = new SimpleScheduler();
            Dictionary<IDevice, DeviceList> lists = simpleChannelScheduler.ScheduleTransmissionList(transmissionList, mockDeviceEventFactory.Object);

            DeviceList deviceOneList = lists[mockDevice1.Object];

            Assert.Contains(deviceOneList.Events, ((deviceEvent) => deviceEvent.RelatedTransmissionListEventId == event1.Id));
            Assert.Contains(deviceOneList.Events, ((deviceEvent) => deviceEvent.RelatedTransmissionListEventId == event2.Id));
            Assert.Contains(deviceOneList.Events, ((deviceEvent) => deviceEvent.RelatedTransmissionListEventId == event4.Id));
            
            Assert.DoesNotContain(deviceOneList.Events, ((deviceEvent) => deviceEvent.RelatedTransmissionListEventId == event3.Id));

            DeviceList deviceTwoList = lists[mockDevice2.Object];

            Assert.Contains(deviceTwoList.Events, ((deviceEvent) => deviceEvent.RelatedTransmissionListEventId == event3.Id));

            Assert.DoesNotContain(deviceTwoList.Events, ((deviceEvent) => deviceEvent.RelatedTransmissionListEventId == event1.Id));
            Assert.DoesNotContain(deviceTwoList.Events, ((deviceEvent) => deviceEvent.RelatedTransmissionListEventId == event2.Id));
            Assert.DoesNotContain(deviceTwoList.Events, ((deviceEvent) => deviceEvent.RelatedTransmissionListEventId == event4.Id));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTransmissionList_ShouldCreateDeviceListWithCorrectOrder()
        {
            var mockDeviceEventFactory = new Mock<IDeviceListEventFactory>();
            mockDeviceEventFactory.Setup(mockDeviceEventFactory => mockDeviceEventFactory.CreateDeviceListEvent(It.IsAny<string>(), It.IsAny<Guid>()))
                                  .Returns((string s, Guid id) => new DeviceListEvent(s, id));

            SimpleScheduler simpleChannelScheduler = new SimpleScheduler();
            Dictionary<IDevice, DeviceList> lists = simpleChannelScheduler.ScheduleTransmissionList(transmissionList, mockDeviceEventFactory.Object);

            DeviceList deviceOneList = lists[mockDevice1.Object];

            Assert.Equal(event1.Id, deviceOneList.Events[0].RelatedTransmissionListEventId);
            Assert.Equal(event2.Id, deviceOneList.Events[1].RelatedTransmissionListEventId);
            Assert.Equal(event4.Id, deviceOneList.Events[2].RelatedTransmissionListEventId);

            DeviceList deviceTwoList = lists[mockDevice2.Object];

            Assert.Equal(event3.Id, deviceTwoList.Events[0].RelatedTransmissionListEventId);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTransmissionList_ShouldCreateOneDeviceListEventsWithCorrectData()
        {
            var mockDeviceEventFactory = new Mock<IDeviceListEventFactory>();
            mockDeviceEventFactory.Setup(mockDeviceEventFactory => mockDeviceEventFactory.CreateDeviceListEvent(It.IsAny<string>(), It.IsAny<Guid>()))
                                  .Returns((string s, Guid id) => new DeviceListEvent(s, id));

            SimpleScheduler simpleChannelScheduler = new SimpleScheduler();
            Dictionary<IDevice, DeviceList> lists = simpleChannelScheduler.ScheduleTransmissionList(transmissionList, mockDeviceEventFactory.Object);

            DeviceList deviceOneList = lists[mockDevice1.Object];
            DeviceListEvent deviceListEvent = deviceOneList.Events[0];

            JsonElement eventDataJson = JsonDocument.Parse(deviceListEvent.EventData).RootElement;
            
            JsonElement timingElement = eventDataJson.GetProperty("timing");
            Assert.Equal(event1.ExpectedStartTime, timingElement.GetProperty("startTime").GetDateTime());
            Assert.Equal(event1.ExpectedDuration, timingElement.GetProperty("duration").GetInt32());

            DateTime expectedEndTime = event1.ExpectedStartTime.AddSeconds(event1.ExpectedDuration.FramesToSeconds());
            Assert.Equal(expectedEndTime, timingElement.GetProperty("endTime").GetDateTime());

            JsonElement sourceElement = eventDataJson.GetProperty("source");
            JsonElement sourceDataElement = sourceElement.GetProperty("strategyData");
            Assert.Equal("mediaSource", sourceDataElement.GetProperty("type").GetString());
            Assert.Equal("test1", sourceDataElement.GetProperty("mediaInstance").GetProperty("name").ToString());
            Assert.Equal(0, sourceDataElement.GetProperty("som").GetInt32());
            Assert.Equal(750, sourceDataElement.GetProperty("eom").GetInt32());
            
            Assert.Equal(event1.Id, deviceListEvent.RelatedTransmissionListEventId);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTransmissionList_ShouldCreateAnEventPerFeature()
        {
            var mockDeviceEventFactory = new Mock<IDeviceListEventFactory>();
            mockDeviceEventFactory.Setup(mockDeviceEventFactory => mockDeviceEventFactory.CreateDeviceListEvent(It.IsAny<string>(), It.IsAny<Guid>()))
                                  .Returns((string s, Guid id) => new DeviceListEvent(s, id));

            SimpleScheduler simpleChannelScheduler = new SimpleScheduler();
            Dictionary<IDevice, DeviceList> lists = simpleChannelScheduler.ScheduleTransmissionList(transmissionList, mockDeviceEventFactory.Object);

            DeviceList deviceTwoList = lists[mockDevice2.Object];
            Assert.Equal(event3.Id, deviceTwoList.Events[0].RelatedTransmissionListEventId);
            
            DeviceList deviceThreeList = lists[mockDevice3.Object];
            Assert.Equal(event3.Id, deviceThreeList.Events[0].RelatedTransmissionListEventId);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTranmissionList_ShouldSetDeviceListEventIdsOnEachTranmissionListEvent()
        {
            var mockDeviceEventFactory = new Mock<IDeviceListEventFactory>();
            mockDeviceEventFactory.Setup(mockDeviceEventFactory => mockDeviceEventFactory.CreateDeviceListEvent(It.IsAny<string>(), It.IsAny<Guid>()))
                                  .Returns((string s, Guid id) => new DeviceListEvent(s, id));

            SimpleScheduler simpleChannelScheduler = new SimpleScheduler();
            Dictionary<IDevice, DeviceList> lists = simpleChannelScheduler.ScheduleTransmissionList(transmissionList, mockDeviceEventFactory.Object);
            DeviceList deviceList = lists[mockDevice1.Object];

            Assert.All(deviceList.Events.Where(listEvent => listEvent.RelatedTransmissionListEventId == event1.Id), 
                       (listEvent) => Assert.Contains(listEvent.Id, event1.RelatedDeviceListEvents));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTranmissionList_ShouldSetTransmissionListEventStatus_ToScheduled()
        {
            var mockDeviceEventFactory = new Mock<IDeviceListEventFactory>();
            mockDeviceEventFactory.Setup(mockDeviceEventFactory => mockDeviceEventFactory.CreateDeviceListEvent(It.IsAny<string>(), It.IsAny<Guid>()))
                                  .Returns((string s, Guid id) => new DeviceListEvent(s, id));

            SimpleScheduler simpleChannelScheduler = new SimpleScheduler();
            _ = simpleChannelScheduler.ScheduleTransmissionList(transmissionList, mockDeviceEventFactory.Object);

            Assert.All(transmissionList.Events, (listEvent) => Assert.Equal(TransmissionListEventState.Status.SCHEDULED, listEvent.EventState.CurrentStatus));
        }
    }
}
