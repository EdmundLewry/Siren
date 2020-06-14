using System.Collections.Generic;
using System.Text.Json;
using CBS.Siren.Controllers;
using CBS.Siren.Time;
using CBS.Siren.Utilities;
using Xunit;

namespace CBS.Siren.Test
{
    public class TransmissionListEventFactoryTest
    {
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void BuildTransmissionListEvent_WithFixedTimingData_CreatesEventWithFixedTiming()
        {
            TimingStrategyCreationDTO timingData = new TimingStrategyCreationDTO(){
                StrategyType = "fixed",
                TargetStartTime = "2020-03-22T00:00:10:05"
            };

            TransmissionListEvent createdEvent = TransmissionListEventFactory.BuildTransmissionListEvent(timingData, new List<JsonElement>(), null);

            FixedStartEventTimingStrategy expectedStrategy = new FixedStartEventTimingStrategy(DateTimeExtensions.FromTimecodeString("2020-03-22T00:00:10:05"));
            Assert.Equal(expectedStrategy, createdEvent.EventTimingStrategy);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void BuildTransmissionListEvent_WithSequentialTimingData_CreatesEventWithSequentialTiming()
        {
            TimingStrategyCreationDTO timingData = new TimingStrategyCreationDTO(){
                StrategyType = "sequential"
            };

            TransmissionListEvent createdEvent = TransmissionListEventFactory.BuildTransmissionListEvent(timingData, new List<JsonElement>(), null);

            SequentialStartEventTimingStrategy expectedStrategy = new SequentialStartEventTimingStrategy();
            Assert.Equal(expectedStrategy, createdEvent.EventTimingStrategy);
        }
    }
}