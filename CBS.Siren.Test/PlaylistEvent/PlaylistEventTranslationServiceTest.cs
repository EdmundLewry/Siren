using Xunit;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;

using CBS.Siren;

namespace CBS.Siren.Test
{
    public class PlaylistEventTranslationServiceTest
    {
        //Will reimplement this functionality and introduce tests at the time
        /*
        [Fact]
        public void TranslateToString_ShouldMatch()
        {
            var mockEventFeature = new Mock<IEventFeature>();

            string timingEventData = "MockStrategy{MockEventTiming=true}";
            var mockEventTimingStrategy = new Mock<IEventTimingStrategy>();
            mockEventTimingStrategy.Setup(mock => mock.ToString()).Returns(timingEventData);

            PlaylistEvent PlaylistEvent = new PlaylistEvent(new List<IEventFeature>() { mockEventFeature.Object }, mockEventTimingStrategy.Object);
            
            String translatedEvent = PlaylistEventExtensions.ToJson(PlaylistEvent).ToString();

            JObject rebuiltEvent = JObject.Parse(translatedEvent);
            Assert.True(rebuiltEvent.ContainsKey("Event"));
            JObject containedEvent = (JObject)rebuiltEvent["Event"];
            
            //We don't really care what this is. Just that it's in there
            Assert.True(containedEvent.ContainsKey("Id"));

            DateTime defaultDateTime = new DateTime();
            Assert.Equal(defaultDateTime.ToString(), (string)containedEvent["StartTime"]);
            Assert.Equal(0, (int)containedEvent["Duration"]);

            JObject timingStrategy = (JObject)containedEvent["EventTimingStrategy"];
            Assert.Equal(timingEventData, (string)timingStrategy["EventData"]);
        }
        */
    }
}