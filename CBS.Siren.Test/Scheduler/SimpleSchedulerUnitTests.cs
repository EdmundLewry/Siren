﻿using CBS.Siren.Time;
using Moq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace CBS.Siren.Test
{
    public class SimpleSchedulerUnitTests
    {

        Mock<IDevice> mockDevice1;
        Mock<IDevice> mockDevice2;
        
        TransmissionListEvent event1;
        TransmissionListEvent event2;
        TransmissionListEvent event3;
        TransmissionListEvent event4;

        public TransmissionList GenerateTransmissionList()
        {
            mockDevice1 = new Mock<IDevice>();
            mockDevice2 = new Mock<IDevice>();

            var mockEventTimingStrategy = new Mock<IEventTimingStrategy>();

            var mockFeature = new Mock<IEventFeature>();
            MediaInstance mediaInstance = new MediaInstance("test1");
            mockFeature.Setup(mock => mock.SourceStrategy).Returns(new MediaSourceStrategy(mediaInstance, 0, 750));
            List<IEventFeature> eventFeatures = new List<IEventFeature>() { mockFeature.Object };

            event1 = new TransmissionListEvent(mockDevice1.Object, mockEventTimingStrategy.Object, eventFeatures);
            event2 = new TransmissionListEvent(mockDevice1.Object, mockEventTimingStrategy.Object, eventFeatures);
            event3 = new TransmissionListEvent(mockDevice2.Object, mockEventTimingStrategy.Object, eventFeatures);
            event4 = new TransmissionListEvent(mockDevice1.Object, mockEventTimingStrategy.Object, eventFeatures);

            event3.EventFeatures.Add(new Mock<IEventFeature>().Object);

            return new TransmissionList(new List<TransmissionListEvent>() {event1, event2, event3, event4}, null);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTransmissionList_ShouldCreateOneDeviceListForEachDevice()
        {
            TransmissionList transmissionList = GenerateTransmissionList();
            Dictionary<IDevice, DeviceList> lists = SimpleChannelScheduler.ScheduleTransmissionList(transmissionList);

            Assert.True(lists.ContainsKey(mockDevice1.Object));
            Assert.True(lists.ContainsKey(mockDevice2.Object));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTransmissionList_ShouldCreateListsOfOnlyEventsRelatedToOneDevice()
        {
            TransmissionList transmissionList = GenerateTransmissionList();
            Dictionary<IDevice, DeviceList> lists = SimpleChannelScheduler.ScheduleTransmissionList(transmissionList);

            DeviceList deviceOneList = lists[mockDevice1.Object];
            Assert.Equal(3, deviceOneList.Events.Count);

            Assert.Contains(deviceOneList.Events, ((deviceEvent) => deviceEvent.RelatedTransmissionListEventId == event1.Id));
            Assert.Contains(deviceOneList.Events, ((deviceEvent) => deviceEvent.RelatedTransmissionListEventId == event2.Id));
            Assert.Contains(deviceOneList.Events, ((deviceEvent) => deviceEvent.RelatedTransmissionListEventId == event4.Id));
            
            Assert.DoesNotContain(deviceOneList.Events, ((deviceEvent) => deviceEvent.RelatedTransmissionListEventId == event3.Id));

            DeviceList deviceTwoList = lists[mockDevice2.Object];
            Assert.Equal(2, deviceTwoList.Events.Count);

            Assert.Contains(deviceTwoList.Events, ((deviceEvent) => deviceEvent.RelatedTransmissionListEventId == event3.Id));

            Assert.DoesNotContain(deviceTwoList.Events, ((deviceEvent) => deviceEvent.RelatedTransmissionListEventId == event1.Id));
            Assert.DoesNotContain(deviceTwoList.Events, ((deviceEvent) => deviceEvent.RelatedTransmissionListEventId == event2.Id));
            Assert.DoesNotContain(deviceTwoList.Events, ((deviceEvent) => deviceEvent.RelatedTransmissionListEventId == event4.Id));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTransmissionList_ShouldCreateDeviceListWithCorrectOrder()
        {
            TransmissionList transmissionList = GenerateTransmissionList();
            Dictionary<IDevice, DeviceList> lists = SimpleChannelScheduler.ScheduleTransmissionList(transmissionList);

            DeviceList deviceOneList = lists[mockDevice1.Object];
            Assert.Equal(3, deviceOneList.Events.Count);

            Assert.Equal(event1.Id, deviceOneList.Events[0].RelatedTransmissionListEventId);
            Assert.Equal(event2.Id, deviceOneList.Events[1].RelatedTransmissionListEventId);
            Assert.Equal(event4.Id, deviceOneList.Events[2].RelatedTransmissionListEventId);

            DeviceList deviceTwoList = lists[mockDevice2.Object];
            Assert.Equal(2, deviceTwoList.Events.Count);

            Assert.Equal(event3.Id, deviceTwoList.Events[0].RelatedTransmissionListEventId);
            Assert.Equal(event3.Id, deviceTwoList.Events[1].RelatedTransmissionListEventId);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTransmissionList_ShouldCreateOneDeviceListEventsWithCorrectData()
        {
            TransmissionList transmissionList = GenerateTransmissionList();
            Dictionary<IDevice, DeviceList> lists = SimpleChannelScheduler.ScheduleTransmissionList(transmissionList);

            DeviceList deviceOneList = lists[mockDevice1.Object];
            DeviceListEvent deviceListEvent = deviceOneList.Events[0];

            JsonElement eventDataJson = JsonDocument.Parse(deviceListEvent.EventData).RootElement;
            
            JsonElement timingElement = eventDataJson.GetProperty("timing");
            Assert.Equal(event1.ExpectedStartTime, timingElement.GetProperty("startTime").GetDateTime());
            Assert.Equal(event1.ExpectedDuration, timingElement.GetProperty("duration").GetInt32());

            int durationAsSeconds = event1.ExpectedDuration / TimeSource.SOURCE_FRAMERATE;
            DateTime expectedEndTime = event1.ExpectedStartTime.AddSeconds(durationAsSeconds);
            Assert.Equal(expectedEndTime, timingElement.GetProperty("endTime").GetDateTime());

            JsonElement sourceElement = eventDataJson.GetProperty("source");
            Assert.Equal("media", sourceElement.GetProperty("type").GetString());
            Assert.Equal("test1", sourceElement.GetProperty("mediaInstance").GetProperty("name").ToString());
            Assert.Equal(0, sourceElement.GetProperty("SOM").GetInt32());
            Assert.Equal(750, sourceElement.GetProperty("EOM").GetInt32());
            
            Assert.Equal(event1.Id, deviceListEvent.RelatedTransmissionListEventId);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTransmissionList_ShouldCreateAnEventPerFeature()
        {
            TransmissionList transmissionList = GenerateTransmissionList();
            Dictionary<IDevice, DeviceList> lists = SimpleChannelScheduler.ScheduleTransmissionList(transmissionList);

            DeviceList deviceTwoList = lists[mockDevice2.Object];
            Assert.Equal(2, deviceTwoList.Events.Count);
        }


    }
}
