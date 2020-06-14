using System.Collections.Generic;
using System.Text.Json;
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
            var timingData = new {
                StrategyType = "fixed",
                TargetStartTime = "2020-03-22T00:00:10:05"
            };
            var timingDataJson = JsonExtensions.SerializeObjectDataToJsonString(timingData);
            TransmissionListEvent createdEvent = TransmissionListEventFactory.BuildTransmissionListEvent(, new List<JsonElement>(), null);

            FixedStartEventTimingStrategy expectedStrategy = new FixedStartEventTimingStrategy(DateTimeExtensions.FromTimecodeString("2020-03-22T00:00:10:05"));
            Assert.Equal(expectedStrategy, createdEvent.EventTimingStrategy);
        }
    }
}