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
        public void EventWatcher_ShouldSubscribeToDevice_OnFirstSubscriptionForADevice()
        {
            var mockDevice = new Mock<IDevice>();
            var mockListener = new Mock<IDeviceListEventStatusChangeListener>();
            DeviceListEventWatcher eventWatcherUnderTest = new DeviceListEventWatcher();

            eventWatcherUnderTest.SubcsribeToDevice(mockListener.Object, mockDevice.Object);

            mockDevice.VerifyAdd(mock => mock.OnDeviceEventStatusChanged += eventWatcherUnderTest.EventStatusChangeHandler);
        }

        [Fact]
        [Trait("TestType","UnitTest")]
        public void EventWatcher_ShouldUnsubscribeFromDevice_OnLastUnsubscriptionForADevice()
        {
            var mockDevice = new Mock<IDevice>();
            var mockListener = new Mock<IDeviceListEventStatusChangeListener>();
            var mockListener2 = new Mock<IDeviceListEventStatusChangeListener>();
            DeviceListEventWatcher eventWatcherUnderTest = new DeviceListEventWatcher();

            eventWatcherUnderTest.SubcsribeToDevice(mockListener.Object, mockDevice.Object);
            eventWatcherUnderTest.SubcsribeToDevice(mockListener2.Object, mockDevice.Object);
            eventWatcherUnderTest.UnsubcsribeFromDevice(mockListener.Object, mockDevice.Object);
            eventWatcherUnderTest.UnsubcsribeFromDevice(mockListener2.Object, mockDevice.Object);

            mockDevice.VerifyRemove(mock => mock.OnDeviceEventStatusChanged -= eventWatcherUnderTest.EventStatusChangeHandler, Times.Once);
        }
        
        [Fact]
        [Trait("TestType","UnitTest")]
        public void EventWatcher_ShouldCallSubscribers_OnDeviceListEventStatusChange()
        {
            var mockDevice = new Mock<IDevice>();
            var mockListener = new Mock<IDeviceListEventStatusChangeListener>();
            DeviceListEventWatcher eventWatcherUnderTest = new DeviceListEventWatcher();

            eventWatcherUnderTest.SubcsribeToDevice(mockListener.Object, mockDevice.Object);

            DeviceListEvent returnEvent = new DeviceListEvent("");
            DeviceListEventState returnState = new DeviceListEventState() { CurrentStatus = DeviceListEventState.Status.CUED };
            DeviceListEventStatusChangeArgs args = new DeviceListEventStatusChangeArgs(returnEvent.Id, returnState);

            eventWatcherUnderTest.OnDeviceListEventStatusChange(mockDevice.Object, args);

            mockListener.Verify(mock => mock.OnDeviceListEventStatusChanged(returnEvent.Id, returnState), Times.Once);
        }
    }
}
