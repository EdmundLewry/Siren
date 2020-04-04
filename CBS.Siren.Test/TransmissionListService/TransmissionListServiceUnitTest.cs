using CBS.Siren.Device;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace CBS.Siren.Test
{
    public class TransmissionListServiceUnitTest
    {
        private TransmissionListEvent GenerateTransmissionEvent(IDevice device)
        {
            var mockFeature = new Mock<IEventFeature>();
            mockFeature.Setup(mock => mock.Device).Returns(device);
            List<IEventFeature> features = new List<IEventFeature>() { mockFeature.Object };

            return new TransmissionListEvent(new Mock<IEventTimingStrategy>().Object, features, null);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenPlayCalled_DeliversEventsToDevices()
        {
            var mockDevice = new Mock<IDevice>();

            TransmissionListEvent event1 = GenerateTransmissionEvent(mockDevice.Object);
            TransmissionListEvent event2 = GenerateTransmissionEvent(mockDevice.Object);

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1, event2 }, null);
            TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, new Mock<ILogger<TransmissionListService>>().Object);

            serviceUnderTest.TransmissionList = transmissionList;

            serviceUnderTest.PlayTransmissionList();

            mockDevice.Verify(mock => mock.SetDeviceList(It.IsAny<DeviceList>()));
        }
    }
}
