using Xunit;
using Moq;

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
            Assert.Equal("ChangeMe", translatedEvent);
        }
    }
}