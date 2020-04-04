using CBS.Siren.Device;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace CBS.Siren.Test
{
    public class TransmissionListServiceUnitTest
    {
        private TransmissionListEvent GenerateTestTransmissionListEvent(IDevice device)
        {
            var eventFeature = new Mock<IEventFeature>();
            eventFeature.Setup(mock => mock.Device).Returns(device);

            return new TransmissionListEvent(new Mock<IEventTimingStrategy>().Object, new List<IEventFeature>() { eventFeature.Object });
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenPlayCalled_DeliversEventsToDevices()
        {
            var mockDevice = new Mock<IDevice>();

            DeviceListEvent event1 = new DeviceListEvent("");
            DeviceListEvent event2 = new DeviceListEvent("");

            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { event1, event2 })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>())).Returns(deviceLists);

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>(), null);
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object, new Mock<IDeviceListEventWatcher>().Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.PlayTransmissionList();

            mockDevice.VerifySet(mock => mock.ActiveList = It.IsAny<DeviceList>());
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenTransmissionListSet_ShouldSubscribeToAllRelevantDevices()
        {
            var mockEventWatcher = new Mock<IDeviceListEventWatcher>();
            var mockDevice1 = new Mock<IDevice>();
            var mockDevice2 = new Mock<IDevice>();

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(mockDevice1.Object);
            TransmissionListEvent event2 = GenerateTestTransmissionListEvent(mockDevice2.Object);

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1, event2 }, null);
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, mockEventWatcher.Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            mockEventWatcher.Verify(mock => mock.SubcsribeToDevice(serviceUnderTest, mockDevice1.Object));
            mockEventWatcher.Verify(mock => mock.SubcsribeToDevice(serviceUnderTest, mockDevice2.Object));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenTransmissionListSet_ShouldUnsubscribeFromAllDevicesFromPreviousList()
        {
            var mockEventWatcher = new Mock<IDeviceListEventWatcher>();
            var mockDevice1 = new Mock<IDevice>();
            var mockDevice2 = new Mock<IDevice>();

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(mockDevice1.Object);
            TransmissionListEvent event2 = GenerateTestTransmissionListEvent(mockDevice2.Object);

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1, event2 }, null);
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, mockEventWatcher.Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            var mockDevice3 = new Mock<IDevice>();
            var mockDevice4 = new Mock<IDevice>();
            TransmissionListEvent event3 = GenerateTestTransmissionListEvent(mockDevice3.Object);
            TransmissionListEvent event4 = GenerateTestTransmissionListEvent(mockDevice4.Object);

            TransmissionList replacementList = new TransmissionList(new List<TransmissionListEvent>() { event3, event4 }, null);
            serviceUnderTest.TransmissionList = replacementList;

            mockEventWatcher.Verify(mock => mock.UnsubcsribeFromDevice(serviceUnderTest, mockDevice1.Object));
            mockEventWatcher.Verify(mock => mock.UnsubcsribeFromDevice(serviceUnderTest, mockDevice2.Object));
        }
    }
}
