using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PBS.Siren
{
    public class TransmissionEventTranslationService
    {
        public static String TranslateToString(TransmissionEvent transmissionEvent)
        {
            JObject eventObject = new JObject(
                                    new JProperty("Event", new JObject(
                                                                    new JProperty("Id", transmissionEvent.Id),
                                                                    new JProperty("StartTime", transmissionEvent.StartTime),
                                                                    new JProperty("Duration", transmissionEvent.Duration),
                                                                    new JProperty("EventTimingStrategy", new JObject(new JProperty("EventData", transmissionEvent.EventTimingStrategy.BuildEventData()))),
                                                                    new JProperty("PlayoutStrategy", new JObject(new JProperty("EventData", transmissionEvent.PlayoutStrategy.BuildEventData()))),
                                                                    new JProperty("SourceStrategy", new JObject(new JProperty("EventData", transmissionEvent.SourceStrategy.BuildEventData()))))));
            return eventObject.ToString();
        } 
    }
}