using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace CBS.Siren.Test.Device
{
    public class DeviceListEventStoreUnitTest
    {
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void UpdateDeviceListEvent_WhenEventDoesNotExist_ThrowsException()
        {
            DeviceListEvent listEvent = new DeviceListEvent("");

            IDeviceListEventStore eventStore = new DeviceListEventStore(new Mock<ILogger<DeviceListEventStore>>().Object);
            Assert.ThrowsAny<Exception>(() => eventStore.UpdateDeviceListEvent(listEvent));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void UpdateDeviceListEvent_WhenEventExists_UpdatesPropertiesOfTheEvent()
        {
            IDeviceListEventStore eventStore = new DeviceListEventStore(new Mock<ILogger<DeviceListEventStore>>().Object);
            DeviceListEvent listEvent = eventStore.CreateDeviceListEvent("", 0);

            DeviceListEvent updatingEvent = new DeviceListEvent(listEvent);
            updatingEvent.EventState.CurrentStatus = DeviceListEventState.Status.PLAYED;
            
            eventStore.UpdateDeviceListEvent(updatingEvent);

            DeviceListEvent updatedListEvent = eventStore.GetEventById(listEvent.Id);
            Assert.Equal(DeviceListEventState.Status.PLAYED, updatedListEvent.EventState.CurrentStatus);
        }
    }
}
