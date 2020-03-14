using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CBS.Siren
{
    public static class PlaylistEventExtensions
    {
        public static JObject ToJson(this PlaylistEvent PlaylistEvent)
        {
            JObject eventObject = new JObject(
                                        new JProperty("Event", 
                                            new JObject(
                                                new JProperty("Id", PlaylistEvent.Id),
                                                new JProperty("EventTimingStrategy", 
                                                    new JObject(
                                                        new JProperty("EventTimingData", PlaylistEvent.EventTimingStrategy.ToString())
                                                    )
                                                )
                                            )
                                        )
                                    );
            return eventObject;
        } 
    }
}