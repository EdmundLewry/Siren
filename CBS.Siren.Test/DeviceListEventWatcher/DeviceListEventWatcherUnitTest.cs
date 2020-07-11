using CBS.Siren.Device;
using Moq;
using System;
using Xunit;

namespace CBS.Siren.Test
{
    public class DeviceListEventWatcherUnitTest
    {
        [Fact]
        [Trait("TestType","UnitTest")]
        public void EventWatcher_ShouldUnsubscribeFromDevice_OnLastUnsubscriptionForADevice()
        {
            var mockDevice = new Mock<IDevice>();
            var mockListener = new Mock<IDeviceListEventStatusChangeListener>();
            var mockListener2 = new Mock<IDeviceListEventStatusChangeListener>();
            using DeviceListEventWatcher eventWatcherUnderTest = new DeviceListEventWatcher();

            eventWatcherUnderTest.SubcsribeToDevice(mockListener.Object, mockDevice.Object);
            eventWatcherUnderTest.SubcsribeToDevice(mockListener2.Object, mockDevice.Object);
            eventWatcherUnderTest.UnsubcsribeFromDevice(mockListener.Object, mockDevice.Object);
            eventWatcherUnderTest.UnsubcsribeFromDevice(mockListener2.Object, mockDevice.Object);

            DeviceListEventStatusChangeArgs args = new DeviceListEventStatusChangeArgs(0);

            eventWatcherUnderTest.OnDeviceListEventStatusChange(mockDevice.Object, args);

            mockListener.Verify(mock => mock.OnDeviceListEventStatusChanged(It.IsAny<int>(), It.IsAny<DeviceListEventState>()), Times.Never);
        }
        
        [Fact]
        [Trait("TestType","UnitTest")]
        public void EventWatcher_ShouldCallSubscribers_OnDeviceListEventStatusChange()
        {
            var mockDevice = new Mock<IDevice>();
            var mockListener = new Mock<IDeviceListEventStatusChangeListener>();
            using DeviceListEventWatcher eventWatcherUnderTest = new DeviceListEventWatcher();

            eventWatcherUnderTest.SubcsribeToDevice(mockListener.Object, mockDevice.Object);

            DeviceListEvent returnEvent = new DeviceListEvent("");
            DeviceListEventState returnState = new DeviceListEventState() { CurrentStatus = DeviceListEventState.Status.CUED };
            DeviceListEventStatusChangeArgs args = new DeviceListEventStatusChangeArgs(returnEvent.Id, returnState);

            eventWatcherUnderTest.OnDeviceListEventStatusChange(mockDevice.Object, args);

            mockListener.Verify(mock => mock.OnDeviceListEventStatusChanged(returnEvent.Id, returnState), Times.Once);
        }
    }
}
