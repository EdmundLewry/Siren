using System;

namespace CBS.Siren
{
    public interface IDeviceListEventFactory
    {
        DeviceListEvent CreateDeviceListEvent(string eventData, Guid associatedTransmissionListEvent);
        DeviceListEvent GetEventById(Guid eventId);
    }
}
