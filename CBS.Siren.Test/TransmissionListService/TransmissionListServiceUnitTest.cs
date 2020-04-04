using CBS.Siren.Device;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace CBS.Siren.Test
{
    public class TransmissionListServiceUnitTest
    {

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenPlayCalled_DeliversEventsToDevices()
        {
            var mockDevice = new Mock<IDevice>();

            DeviceListEvent event1 = new DeviceListEvent("");
            DeviceListEvent event2 = new DeviceListEvent("");

            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>() { 
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { event1, event2 })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>())).Returns(deviceLists);

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>(), null);
            TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object, new Mock<ILogger<TransmissionListService>>().Object);

            serviceUnderTest.TransmissionList = transmissionList;

            serviceUnderTest.PlayTransmissionList();

            mockDevice.VerifySet(mock => mock.ActiveList = It.IsAny<DeviceList>());
        }
    }
}
