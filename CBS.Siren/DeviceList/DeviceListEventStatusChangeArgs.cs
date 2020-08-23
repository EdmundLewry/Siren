namespace CBS.Siren
{
    public class DeviceListEventStatusChangeArgs
    {
        public DeviceListEventState NewState { get; set; }
        public int EventId { get; set; }
        public int? RelatedTransmissionListEventId { get; set; }
        public DeviceListEventStatusChangeArgs(int eventId, int? transmissionListEvent = null, DeviceListEventState newState = null)
        {
            NewState = newState;
            EventId = eventId;
            RelatedTransmissionListEventId = transmissionListEvent;
        }
    }
}
