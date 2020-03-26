using CBS.Siren.Device;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CBS.Siren.Test.Device
{
    public class DemoDeviceUnitTest
    {
        [Fact]
        [Trait("TestType","UnitTest")]
        public void OnCreation_DeviceReportsStopped()
        {
            var mockDriver = new Mock<IDeviceDriver>().Object;
            var mockController = new Mock<IDeviceController>().Object;

            using IDevice device = new DemoDevice("test", mockController, mockDriver);
            Assert.Equal(IDevice.DeviceStatus.STOPPED, device.CurrentStatus);
        }
        
        [Fact]
        [Trait("TestType","UnitTest")]
        public void WhenController_ReturnsCurrentEvent_DeviceReportsPlaying()
        {
            var mockDriver = new Mock<IDeviceDriver>().Object;
            var mockController = new Mock<IDeviceController>();

            DeviceListEvent returnEvent = new DeviceListEvent("");
            mockController.Setup(mock => mock.CurrentEvent).Returns(returnEvent);

            using IDevice device = new DemoDevice("test", mockController.Object, mockDriver);

            int callCount = 0;
            EventHandler<DeviceStatusEventArgs> eventHandler = new EventHandler<DeviceStatusEventArgs>((sender, args) => {
                if(args.NewStatus == IDevice.DeviceStatus.PLAYING)
                {
                    callCount++;
                }
            });
            device.OnDeviceStatusChanged += eventHandler;

            mockController.Raise(mock => mock.OnEventStarted += null, new DeviceEventChangedEventArgs(returnEvent));
            
            Assert.Equal(1, callCount);

            device.OnDeviceStatusChanged -= eventHandler;
        }
        
        [Fact]
        [Trait("TestType","UnitTest")]
        public void WhenDevice_WasPlaying_AndCurrentEventIsNull_DeviceReportsStopped()
        {
            var mockDriver = new Mock<IDeviceDriver>().Object;
            var mockController = new Mock<IDeviceController>();

            DeviceListEvent returnEvent = new DeviceListEvent("");
            mockController.Setup(mock => mock.CurrentEvent).Returns(returnEvent);

            using IDevice device = new DemoDevice("test", mockController.Object, mockDriver);

            int callCount = 0;
            EventHandler<DeviceStatusEventArgs> eventHandler = new EventHandler<DeviceStatusEventArgs>((sender, args) => {
                if (args.NewStatus == IDevice.DeviceStatus.PLAYING)
                {
                    returnEvent = null;
                    mockController.Setup(mock => mock.CurrentEvent).Returns(returnEvent);
                }

                if(args.NewStatus == IDevice.DeviceStatus.STOPPED)
                {
                    callCount++;
                }
            });
            device.OnDeviceStatusChanged += eventHandler;

            mockController.Raise(mock => mock.OnEventStarted += null, new DeviceEventChangedEventArgs(returnEvent));
            mockController.Raise(mock => mock.OnDeviceListEnded += null, EventArgs.Empty);

            Assert.Equal(1, callCount);

            device.OnDeviceStatusChanged -= eventHandler;
        }
    }
}
