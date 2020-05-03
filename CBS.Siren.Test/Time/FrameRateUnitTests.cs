using CBS.Siren.Time;
using Xunit;

namespace CBS.Siren.Test.Time
{
    public class FrameRateUnitTests
    {
        [Theory]
        [Trait("TestType", "UnitTest")]
        [InlineData(FrameRate.DF30)]
        public void IsDropFrame_GivenADropDrameFrameRate_ShouldReportTrue(FrameRate frameRate)
        {
            Assert.True(frameRate.IsDropFrame());
        }
        
        [Theory]
        [Trait("TestType", "UnitTest")]
        [InlineData(FrameRate.FPS24)]
        [InlineData(FrameRate.FPS25)]
        [InlineData(FrameRate.FPS30)]
        public void IsDropFrame_GivenANonDropDrameFrameRate_ShouldReportFalse(FrameRate frameRate)
        {
            Assert.False(frameRate.IsDropFrame());
        }
    }
}
