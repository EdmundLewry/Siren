namespace CBS.Siren
{
    public class DeviceListEventStatusChangeArgs
    {
        public DeviceListEventState NewState { get; set; }
        public int EventId { get; set; }
        public DeviceListEventStatusChangeArgs(int eventId, DeviceListEventState newState = null)
        {
            NewState = newState;
            EventId = eventId;
        }
    }
}
