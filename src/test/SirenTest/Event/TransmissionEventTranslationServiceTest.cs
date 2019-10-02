using Xunit;
using Moq;
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
            var mockSourceStrategy = new Mock<ISourceStrategy>();
            mockSourceStrategy.Setup(mock => mock.BuildEventData()).Returns("MockStrategy{MockSource=true}");

            var mockPlayoutStrategy = new Mock<IPlayoutStrategy>();
            mockPlayoutStrategy.Setup(mock => mock.BuildEventData()).Returns("MockStrategy{MockPlayout=true}");
            
            var mockEventTimingStrategy = new Mock<IEventTimingStrategy>();
            mockEventTimingStrategy.Setup(mock => mock.BuildEventData()).Returns("MockStrategy{MockEventTiming=true}");

            TransmissionEvent transmissionEvent = new TransmissionEvent(mockSourceStrategy.Object, mockPlayoutStrategy.Object, mockEventTimingStrategy.Object);
            
            String translatedEvent = TransmissionEventTranslationService.TranslateToString(transmissionEvent);

            //Need to create the proper json object here!
            //Slight problem. How do I handle the fact that the id (and other meta data) might be generated.
            //I could pass it in. I could use an interface and mock that. I could convert the object a json object
            //and just query the properties?
            
        }
    }
}