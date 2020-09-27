using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace CBS.Siren
{
    //For now, we won't worry about cleaning this store of created events out.
    //Eventually we should create some purge mechanism, but we may wait for a real data store before
    //we do that
    public class DeviceListEventStore : IDeviceListEventStore
    {
        private readonly ILogger<DeviceListEventStore> _logger;
        public ConcurrentDictionary<int, DeviceListEvent> CreatedEvents { get; private set; } = new ConcurrentDictionary<int, DeviceListEvent>();

        public DeviceListEventStore(ILogger<DeviceListEventStore> logger)
        {
            _logger = logger;
        }
        
        public DeviceListEvent CreateDeviceListEvent(string eventData, int associatedTransmissionListEvent)
        {
            DeviceListEvent deviceListEvent = new DeviceListEvent(eventData, associatedTransmissionListEvent);
            DeviceListEvent foundListEvent = CreatedEvents.GetOrAdd(deviceListEvent.Id, deviceListEvent);
            _logger.LogTrace($"Created DeviceListEvent with Id {foundListEvent.Id}");
            return foundListEvent;
        }

        public DeviceListEvent GetEventById(int eventId)
        {
            return CreatedEvents.ContainsKey(eventId) ? CreatedEvents[eventId] : null;
        }

        public DeviceListEvent UpdateDeviceListEvent(DeviceListEvent deviceListEvent)
        {
            DeviceListEvent foundEvent = GetEventById(deviceListEvent.Id);
            if(foundEvent == null)
            {
                throw new ArgumentException($"Unable to update device list event with id {deviceListEvent.Id}");
            }

            _logger.LogTrace($"Updating DeviceListEvents {deviceListEvent.Id}");
            bool success = CreatedEvents.TryUpdate(foundEvent.Id, deviceListEvent, foundEvent);
            if(!success)
            {
                throw new InvalidOperationException($"Failed to update DeviceListEvent store with latest data for event '{deviceListEvent.Id}'");
            }
            return GetEventById(foundEvent.Id);
        }
    }
}
