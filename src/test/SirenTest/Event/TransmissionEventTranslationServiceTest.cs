using Xunit;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;

using PBS.Siren;

namespace SirenTest
{
    public class TransmissionEventTranslationServiceTest
    {
        public TransmissionEventTranslationServiceTest()
        {
            
        }

        [Fact]
        public void TranslateToString_ShouldMatch()
        {
            string sourceEventData = "MockStrategy{MockSource=true}";
            var mockSourceStrategy = new Mock<ISourceStrategy>();
            mockSourceStrategy.Setup(mock => mock.BuildEventData()).Returns(sourceEventData);

            string playoutEventData = "MockStrategy{MockPlayout=true}";
            var mockPlayoutStrategy = new Mock<IPlayoutStrategy>();
            mockPlayoutStrategy.Setup(mock => mock.BuildEventData()).Returns(playoutEventData);
            
            string timingEventData = "MockStrategy{MockEventTiming=true}";
            var mockEventTimingStrategy = new Mock<IEventTimingStrategy>();
            mockEventTimingStrategy.Setup(mock => mock.BuildEventData()).Returns(timingEventData);

            TransmissionEvent transmissionEvent = new TransmissionEvent(mockSourceStrategy.Object, mockPlayoutStrategy.Object, mockEventTimingStrategy.Object);
            
            String translatedEvent = TransmissionEventTranslationService.TranslateToString(transmissionEvent);

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

            JObject playoutStrategy = (JObject)containedEvent["PlayoutStrategy"];
            Assert.Equal(playoutEventData, (string)playoutStrategy["EventData"]);

            JObject sourceStrategy = (JObject)containedEvent["SourceStrategy"];
            Assert.Equal(sourceEventData, (string)sourceStrategy["EventData"]);
        }
    }
}