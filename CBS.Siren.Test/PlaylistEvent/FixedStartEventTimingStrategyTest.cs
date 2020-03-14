using Xunit;
using Moq;

using System;
using System.Collections.Generic;

using CBS.Siren;

namespace CBS.Siren.Test
{
    public class FixedStartEventTimingStrategyTest
    {
        public FixedStartEventTimingStrategyTest()
        {
            
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void CalculateStartTime_ReportsGivenTarget()
        {
            const String targetTimeString = "01/10/2019 08:00:00 AM";
            DateTime target = DateTime.Parse(targetTimeString);
            FixedStartEventTimingStrategy strategy = new FixedStartEventTimingStrategy(target);

            DateTime startTime = strategy.CalculateStartTime();
            Assert.Equal(target, startTime);
        }
    }
}