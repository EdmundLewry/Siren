using System;

namespace CBS.Siren
{
    public class DeviceListEventStatusChangeArgs
    {
        public DeviceListEventState NewState { get; set; }
        public Guid EventId { get; set; }
        public DeviceListEventStatusChangeArgs(Guid eventId, DeviceListEventState newState = null)
        {
            NewState = newState;
            EventId = eventId;
        }
    }
}
