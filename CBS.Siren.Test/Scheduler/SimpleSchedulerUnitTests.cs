using Moq;
using System;
using System.Collections.Generic;
using System.Text;
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

            var mockSourceStrategy = new Mock<ISourceStrategy>();
            var mockPlayoutStrategy = new Mock<IPlayoutStrategy>();
            var mockEventTimingStrategy = new Mock<IEventTimingStrategy>();

            var mockFeature = new Mock<IEventFeature>();
            List<IEventFeature> eventFeatures = new List<IEventFeature>() { mockFeature.Object };

            event1 = new TransmissionListEvent(mockDevice1.Object, mockEventTimingStrategy.Object, eventFeatures);
            event2 = new TransmissionListEvent(mockDevice1.Object, mockEventTimingStrategy.Object, eventFeatures);
            event3 = new TransmissionListEvent(mockDevice2.Object, mockEventTimingStrategy.Object, eventFeatures);
            event4 = new TransmissionListEvent(mockDevice1.Object, mockEventTimingStrategy.Object, eventFeatures);

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

            //TODO:4 Check list contains correct events

            DeviceList deviceTwoList = lists[mockDevice2.Object];
            Assert.Single(deviceTwoList.Events);

            //TODO:4 Check list contains correct events
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTransmissionList_ShouldCreateOneDeviceListWithCorrectOrder()
        {
            TransmissionList transmissionList = GenerateTransmissionList();
            Dictionary<IDevice, DeviceList> lists = SimpleChannelScheduler.ScheduleTransmissionList(transmissionList);

            
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void ScheduleTransmissionList_ShouldCreateOneDeviceListEventsWithCorrectData()
        {
            TransmissionList transmissionList = GenerateTransmissionList();
            Dictionary<IDevice, DeviceList> lists = SimpleChannelScheduler.ScheduleTransmissionList(transmissionList);

            
        }


    }
}
