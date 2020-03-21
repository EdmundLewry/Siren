using System;

namespace CBS.Siren
{
    /*
    A DeviceListEvent is a lightweight container for the data required by a specific device in order for
    that device to be able to enact the event at the correct time. 
    */
    public class DeviceListEvent
    {
        //We may want more human readable identifiers
        public Guid Id { get; set; }
        public Guid? RelatedTransmissionListEventId { get; set; }

        public DeviceListEventState EventState { get; set; }
        public string EventData {get;}

        public DeviceListEvent(String eventData, Guid? relatedEventId = null)
        {
            Id = Guid.NewGuid();
            RelatedTransmissionListEventId = relatedEventId;
            EventData = eventData;
        }
    }
}