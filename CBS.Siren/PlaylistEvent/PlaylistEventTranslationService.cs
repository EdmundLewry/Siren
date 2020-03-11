using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CBS.Siren
{
    public class PlaylistEventTranslationService
    {
        public static String TranslateToString(PlaylistEvent PlaylistEvent)
        {
            JObject eventObject = new JObject(
                                    new JProperty("Event", new JObject(
                                                                    new JProperty("Id", PlaylistEvent.Id),
                                                                    new JProperty("StartTime", PlaylistEvent.StartTime),
                                                                    new JProperty("Duration", PlaylistEvent.Duration),
                                                                    new JProperty("EventTimingStrategy", new JObject(new JProperty("EventData", PlaylistEvent.EventTimingStrategy.BuildEventData()))),
                                                                    new JProperty("PlayoutStrategy", new JObject(new JProperty("EventData", PlaylistEvent.PlayoutStrategy.BuildEventData()))),
                                                                    new JProperty("SourceStrategy", new JObject(new JProperty("EventData", PlaylistEvent.SourceStrategy.BuildEventData()))))));
            return eventObject.ToString();
        } 
    }
}