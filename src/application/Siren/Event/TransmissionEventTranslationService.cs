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
                                    new JProperty("event", new JObject(
                                                                    new JProperty("Id", transmissionEvent.Id),
                                                                    new JProperty("StartTime", transmissionEvent.StartTime),
                                                                    new JProperty("Duration", transmissionEvent.Duration),
                                                                    new JObject(new JProperty("EventTimingStrategy", transmissionEvent.EventTimingStrategy.BuildEventData())),
                                                                    new JObject(new JProperty("PlayoutStrategy", transmissionEvent.PlayoutStrategy.BuildEventData())),
                                                                    new JObject(new JProperty("SourceStrategy", transmissionEvent.SourceStrategy.BuildEventData())))));
            return "";
        } 
    }
}