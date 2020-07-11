using System;
using System.Collections.Generic;

namespace CBS.Siren
{
    //For now, we won't worry about cleaning this store of created events out.
    //Eventually we should create some purge mechanism, but we may wait for a real data store before
    //we do that
    public class DeviceListEventFactory : IDeviceListEventFactory
    {
        public Dictionary<Guid, DeviceListEvent> CreatedEvents { get; private set; } = new Dictionary<Guid, DeviceListEvent>();

        public DeviceListEvent CreateDeviceListEvent(string eventData, int associatedTransmissionListEvent)
        {
            DeviceListEvent deviceListEvent = new DeviceListEvent(eventData, associatedTransmissionListEvent);
            CreatedEvents.Add(deviceListEvent.Id, deviceListEvent);
            return deviceListEvent;
        }

        public DeviceListEvent GetEventById(Guid eventId)
        {
            return CreatedEvents.ContainsKey(eventId) ? CreatedEvents[eventId] : null;
        }
    }
}
