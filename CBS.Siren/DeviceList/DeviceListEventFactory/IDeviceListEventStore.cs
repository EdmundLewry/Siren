namespace CBS.Siren
{
    public interface IDeviceListEventStore
    {
        DeviceListEvent CreateDeviceListEvent(string eventData, int associatedTransmissionListEvent);
        DeviceListEvent UpdateDeviceListEvent(DeviceListEvent deviceListEvent);
        DeviceListEvent GetEventById(int eventId);
    }
}
