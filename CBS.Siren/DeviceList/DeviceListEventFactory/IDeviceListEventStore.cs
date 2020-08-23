namespace CBS.Siren
{
    public interface IDeviceListEventStore
    {
        DeviceListEvent CreateDeviceListEvent(string eventData, int associatedTransmissionListEvent);
        DeviceListEvent GetEventById(int eventId);
    }
}
