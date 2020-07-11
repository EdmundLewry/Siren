using CBS.Siren.Time;
using System;
using System.Text.Json;

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
        public int? RelatedTransmissionListEventId { get; set; }

        public DeviceListEventState EventState { get; set; } = new DeviceListEventState();
        public string EventData {get;}
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public DeviceListEvent(string eventData, int? relatedEventId = null)
        {
            Id = Guid.NewGuid();
            RelatedTransmissionListEventId = relatedEventId;
            EventData = eventData;
            ProcessEventData();
        }

        private void ProcessEventData()
        {
            //Should do better error handling here
            try
            {
                //I wonder if we should try to separate JSON from the Device Event?
                JsonElement timingElement = JsonDocument.Parse(EventData).RootElement.GetProperty("timing");
                StartTime = DateTimeExtensions.FromTimecodeString(timingElement.GetProperty("startTime").GetString());
                EndTime = DateTimeExtensions.FromTimecodeString(timingElement.GetProperty("endTime").GetString());
            }
            catch
            {
                Console.WriteLine("Unable to parse event data");
                StartTime = DateTime.MaxValue;
                EndTime = DateTime.MaxValue;
            }
        }

        public override String ToString()
        {
            return  base.ToString() +
                    $":\nId: {Id}" +
                    $"\nRelated Transmission List Event Id: {RelatedTransmissionListEventId}" +
                    $"\nEventData: {EventData}" +
                    $"\nEventStatus: {EventState}";
        }
    }
}