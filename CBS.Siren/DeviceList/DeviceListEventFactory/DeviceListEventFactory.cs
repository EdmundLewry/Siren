using System;
using System.Collections.Generic;

namespace CBS.Siren
{
    public class DeviceListEventFactory : IDeviceListEventFactory
    {
        public Dictionary<Guid, DeviceListEvent> CreatedEvents { get; private set; } = new Dictionary<Guid, DeviceListEvent>();

        public DeviceListEvent CreateDeviceListEvent(string eventData, Guid associatedTransmissionListEvent)
        {
            DeviceListEvent deviceListEvent = new DeviceListEvent(eventData, associatedTransmissionListEvent);
            CreatedEvents.Add(deviceListEvent.Id, deviceListEvent);
            return deviceListEvent;
        }
    }
}
