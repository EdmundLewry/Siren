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

            IDevice device = new DemoDevice("test", mockController, mockDriver);
            Assert.Equal(IDevice.DeviceStatus.STOPPED, device.CurrentStatus);
        }
        
        [Fact]
        [Trait("TestType","UnitTest")]
        public async Task WhenController_ReturnsCurrentEvent_DeviceReportsPlaying_Once()
        {
            var mockDriver = new Mock<IDeviceDriver>().Object;
            var mockController = new Mock<IDeviceController>();

            DeviceListEvent returnEvent = new DeviceListEvent("");
            mockController.Setup(mock => mock.CurrentEvent).Returns(returnEvent);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            IDevice device = new DemoDevice("test", mockController.Object, mockDriver);

            int callCount = 0;
            EventHandler<DeviceStatusEventArgs> eventHandler = new EventHandler<DeviceStatusEventArgs>((sender, args) => {
                if(args.NewStatus == IDevice.DeviceStatus.PLAYING)
                {
                    callCount++;
                }
            });
            device.OnDeviceStatusChanged += eventHandler;

            Task runningTask = Task.Run(() => device.Run(cancellationTokenSource.Token));

            await Task.Delay(10);
            
            Assert.Equal(1, callCount);

            device.OnDeviceStatusChanged -= eventHandler;
            if(cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }
            await runningTask;
        }
        
        [Fact]
        [Trait("TestType","UnitTest")]
        public async Task WhenDevice_WasPlaying_AndCurrentEventIsNull_DeviceReportsStopped_Once()
        {
            var mockDriver = new Mock<IDeviceDriver>().Object;
            var mockController = new Mock<IDeviceController>();

            DeviceListEvent returnEvent = new DeviceListEvent("");
            mockController.Setup(mock => mock.CurrentEvent).Returns(returnEvent);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            IDevice device = new DemoDevice("test", mockController.Object, mockDriver);

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

            Task runningTask = Task.Run(() => device.Run(cancellationTokenSource.Token));

            await Task.Delay(10); //Allow the thread some time to work

            Assert.Equal(1, callCount);

            device.OnDeviceStatusChanged -= eventHandler;
            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }
            await runningTask;
        }
    }
}
