using CBS.Siren.Device;
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

        private TransmissionList GenerateTransmissionList()
        {
            var mockEventTimingStrategy = new Mock<IEventTimingStrategy>();
            mockEventTimingStrategy.Setup(mock => mock.CalculateStartTime(It.IsAny<int>(), It.IsAny<TransmissionList>())).Returns(DateTime.Parse("01/01/2020 14:30:00"));

            event1 = new TransmissionListEvent(mockEventTimingStrategy.Object, GenerateMockFeatureList(mockDevice1.Object));
            event2 = new TransmissionListEvent(mockEventTimingStrategy.Object, GenerateMockFeatureList(mockDevice1.Object));
            event3 = new TransmissionListEvent(mockEventTimingStrategy.Object, GenerateMockFeatureList(mockDevice2.Object, mockDevice3.Object));
            event4 = new TransmissionListEvent(mockEventTimingStrategy.Object, GenerateMockFeatureList(mockDevice1.Object));

            return new TransmissionList(new List<TransmissionListEvent>() {event1, event2, event3, event4}, null);
        }

        private List<IEventFeature> GenerateMockFeatureList(params IDevice[] devices)
        {
            List<IEventFeature> eventFeatures = new List<IEventFeature>();
            devices.ToList().ForEach(device => eventFeatures.Add(GenerateMockFeature(device).Object));

            return eventFeatures;
        }

        private Mock<IEventFeature> GenerateMockFeature(IDevice device)
        {
            var mockFeature = new Mock<IEventFeature>();
            MediaInstance mediaInstance = new MediaInstance("test1", TimeSpan.Zero);
            mockFeature.Setup(mock => mock.SourceStrategy).Returns(new MediaSourceStrategy(mediaInstance, TimeSpan.Zero, new TimeSpan(0, 0, 30)));
            mockFeature.Setup(mock => mock.CalculateDuration()).Returns(new TimeSpan(0, 0, 30));
            mockFeature.Setup(mock => mock.Device).Returns(device);
            mockFeature.SetupProperty(mock => mock.DeviceListEventId);

            return mockFeature;
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTransmissionList_ShouldCreateOneDeviceListForEachDevice()
        {
            var mockDeviceEventStore = new Mock<IDeviceListEventStore>();
            mockDeviceEventStore.Setup(mockDeviceEventStore => mockDeviceEventStore.CreateDeviceListEvent(It.IsAny<string>(), It.IsAny<int>()))
                                  .Returns((string s, int id) => new DeviceListEvent(s, id));

            SimpleScheduler simpleChannelScheduler = new SimpleScheduler();
            Dictionary<IDevice, DeviceList> lists = simpleChannelScheduler.ScheduleTransmissionList(transmissionList, mockDeviceEventStore.Object);

            Assert.True(lists.ContainsKey(mockDevice1.Object));
            Assert.True(lists.ContainsKey(mockDevice2.Object));
            Assert.True(lists.ContainsKey(mockDevice3.Object));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTransmissionList_ShouldCreateListsOfOnlyEventsRelatedToOneDevice()
        {
            var mockDeviceEventStore = new Mock<IDeviceListEventStore>();
            mockDeviceEventStore.Setup(mockDeviceEventStore => mockDeviceEventStore.CreateDeviceListEvent(It.IsAny<string>(), It.IsAny<int>()))
                                  .Returns((string s, int id) => new DeviceListEvent(s, id));

            SimpleScheduler simpleChannelScheduler = new SimpleScheduler();
            Dictionary<IDevice, DeviceList> lists = simpleChannelScheduler.ScheduleTransmissionList(transmissionList, mockDeviceEventStore.Object);

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
            var mockDeviceEventStore = new Mock<IDeviceListEventStore>();
            mockDeviceEventStore.Setup(mockDeviceEventStore => mockDeviceEventStore.CreateDeviceListEvent(It.IsAny<string>(), It.IsAny<int>()))
                                  .Returns((string s, int id) => new DeviceListEvent(s, id));

            SimpleScheduler simpleChannelScheduler = new SimpleScheduler();
            Dictionary<IDevice, DeviceList> lists = simpleChannelScheduler.ScheduleTransmissionList(transmissionList, mockDeviceEventStore.Object);

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
            var mockDeviceEventStore = new Mock<IDeviceListEventStore>();
            mockDeviceEventStore.Setup(mockDeviceEventStore => mockDeviceEventStore.CreateDeviceListEvent(It.IsAny<string>(), It.IsAny<int>()))
                                  .Returns((string s, int id) => new DeviceListEvent(s, id));

            SimpleScheduler simpleChannelScheduler = new SimpleScheduler();
            Dictionary<IDevice, DeviceList> lists = simpleChannelScheduler.ScheduleTransmissionList(transmissionList, mockDeviceEventStore.Object);

            DeviceList deviceOneList = lists[mockDevice1.Object];
            DeviceListEvent deviceListEvent = deviceOneList.Events[0];

            JsonElement eventDataJson = JsonDocument.Parse(deviceListEvent.EventData).RootElement;
            
            JsonElement timingElement = eventDataJson.GetProperty("timing");
            Assert.Equal(event1.ExpectedStartTime, DateTimeExtensions.FromTimecodeString(timingElement.GetProperty("startTime").GetString()));
            Assert.Equal(event1.ExpectedDuration, TimeSpanExtensions.FromTimecodeString(timingElement.GetProperty("duration").GetString()));

            DateTime expectedEndTime = event1.ExpectedStartTime.AddSeconds(event1.ExpectedDuration.TotalSeconds);
            Assert.Equal(expectedEndTime, DateTimeExtensions.FromTimecodeString(timingElement.GetProperty("endTime").GetString()));

            JsonElement sourceElement = eventDataJson.GetProperty("source");
            JsonElement sourceDataElement = sourceElement.GetProperty("strategyData");
            Assert.Equal("mediaSource", sourceDataElement.GetProperty("type").GetString());
            Assert.Equal("test1", sourceDataElement.GetProperty("mediaInstance").GetProperty("name").ToString());
            Assert.Equal(0, TimeSpanExtensions.FromTimecodeString(sourceDataElement.GetProperty("som").GetString()).TotalFrames());
            Assert.Equal(750, TimeSpanExtensions.FromTimecodeString(sourceDataElement.GetProperty("eom").GetString()).TotalFrames());
            
            Assert.Equal(event1.Id, deviceListEvent.RelatedTransmissionListEventId);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTransmissionList_ShouldCreateAnEventPerFeature()
        {
            var mockDeviceEventStore = new Mock<IDeviceListEventStore>();
            mockDeviceEventStore.Setup(mockDeviceEventStore => mockDeviceEventStore.CreateDeviceListEvent(It.IsAny<string>(), It.IsAny<int>()))
                                  .Returns((string s, int id) => new DeviceListEvent(s, id));

            SimpleScheduler simpleChannelScheduler = new SimpleScheduler();
            Dictionary<IDevice, DeviceList> lists = simpleChannelScheduler.ScheduleTransmissionList(transmissionList, mockDeviceEventStore.Object);

            DeviceList deviceTwoList = lists[mockDevice2.Object];
            Assert.Equal(event3.Id, deviceTwoList.Events[0].RelatedTransmissionListEventId);
            
            DeviceList deviceThreeList = lists[mockDevice3.Object];
            Assert.Equal(event3.Id, deviceThreeList.Events[0].RelatedTransmissionListEventId);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTranmissionList_ShouldSetDeviceListEventIdsOnEachTranmissionListEvent()
        {
            var mockDeviceEventStore = new Mock<IDeviceListEventStore>();
            mockDeviceEventStore.Setup(mockDeviceEventStore => mockDeviceEventStore.CreateDeviceListEvent(It.IsAny<string>(), It.IsAny<int>()))
                                  .Returns((string s, int id) => new DeviceListEvent(s, id));

            SimpleScheduler simpleChannelScheduler = new SimpleScheduler();
            Dictionary<IDevice, DeviceList> lists = simpleChannelScheduler.ScheduleTransmissionList(transmissionList, mockDeviceEventStore.Object);
            DeviceList deviceList = lists[mockDevice1.Object];

            Assert.All(deviceList.Events.Where(deviceListEvent => deviceListEvent.RelatedTransmissionListEventId == event1.Id), 
                       (deviceListEvent) => Assert.Contains(deviceListEvent.Id, event1.EventFeatures.Select(feature => feature.DeviceListEventId)));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTranmissionList_ShouldSetTransmissionListEventStatus_ToScheduled()
        {
            var mockDeviceEventStore = new Mock<IDeviceListEventStore>();
            mockDeviceEventStore.Setup(mockDeviceEventStore => mockDeviceEventStore.CreateDeviceListEvent(It.IsAny<string>(), It.IsAny<int>()))
                                  .Returns((string s, int id) => new DeviceListEvent(s, id));

            SimpleScheduler simpleChannelScheduler = new SimpleScheduler();
            _ = simpleChannelScheduler.ScheduleTransmissionList(transmissionList, mockDeviceEventStore.Object);

            Assert.All(transmissionList.Events, (listEvent) => Assert.Equal(TransmissionListEventState.Status.SCHEDULED, listEvent.EventState.CurrentStatus));
        }
    }
}
