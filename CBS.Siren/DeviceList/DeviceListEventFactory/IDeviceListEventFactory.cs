namespace CBS.Siren
{
    public interface IDeviceListEventFactory
    {
        DeviceListEvent CreateDeviceListEvent(string eventData, int associatedTransmissionListEvent);
        DeviceListEvent GetEventById(int eventId);
    }
}
