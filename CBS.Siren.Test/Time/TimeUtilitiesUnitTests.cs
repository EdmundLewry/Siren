using System;
using Xunit;
using CBS.Siren.Time;

namespace CBS.Siren.Test.Time
{
    public class TimeUtilitiesUnitTests
    {
        [Fact]
        [Trait("TestType","UnitTest")]
        public void DifferenceInFrames_ReportsPositive_WhenDateTimeIsBeforeArgument()
        {
            DateTimeOffset before = DateTimeOffset.Parse("01/01/2015 00:00:05");
            DateTimeOffset after = DateTimeOffset.Parse("01/01/2015 00:00:10");

            long numFramesDifference = before.DifferenceInFrames(after);
            Assert.True(numFramesDifference > 0);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void DifferenceInFrames_ReportsNegative_WhenDateTimeIsAfterArgument()
        {
            DateTimeOffset before = DateTimeOffset.Parse("01/01/2015 00:00:10");
            DateTimeOffset after = DateTimeOffset.Parse("01/01/2015 00:00:05");

            long numFramesDifference = before.DifferenceInFrames(after);
            Assert.True(numFramesDifference < 0);
        }

        [Theory]
        [Trait("TestType", "UnitTest")]
        [InlineData("01/01/2015 00:00:10")]
        [InlineData("01/01/2015 00:00:10.010")]
        public void DifferenceInFrames_ReportsZero_WhenDateTimeIsOnSameFrame(string comparison)
        {
            DateTimeOffset before = DateTimeOffset.Parse("01/01/2015 00:00:10.000");
            DateTimeOffset after = DateTimeOffset.Parse(comparison);

            long numFramesDifference = before.DifferenceInFrames(after);
            Assert.True(numFramesDifference == 0);
        }

        [Theory]
        [Trait("TestType", "UnitTest")]
        [InlineData("01/01/2015 00:00:10", 125)]
        [InlineData("01/01/2015 00:00:05.050", 1)]
        [InlineData("01/01/2015 00:00:04.919", -3)]
        public void DifferenceInFrames_ReportsCorrectFrameCount(string comparison, int expected)
        {
            DateTimeOffset before = DateTimeOffset.Parse("01/01/2015 00:00:05");
            DateTimeOffset after = DateTimeOffset.Parse(comparison);

            long numFramesDifference = before.DifferenceInFrames(after);
            Assert.Equal(expected, numFramesDifference);
        }

        [Theory]
        [Trait("TestType", "UnitTest")]
        [InlineData(0, 0)]
        [InlineData(10, 0)]
        [InlineData(100, 4)]
        public void FramesToSeconds_ReportsCorrectNumberOfSeconds(int frames, int seconds)
        {
            Assert.Equal(seconds, frames.FramesToSeconds(FrameRate.FPS25));
        }
        
        [Theory]
        [Trait("TestType", "UnitTest")]
        [InlineData(0, 0, FrameRate.FPS25)]
        [InlineData(20, 0, FrameRate.FPS25)]
        [InlineData(40, 1, FrameRate.FPS25)]
        [InlineData(200, 5, FrameRate.FPS25)]
        [InlineData(200, 4, FrameRate.FPS24)]
        [InlineData(200, 6, FrameRate.FPS30)]
        [InlineData(200, 5, FrameRate.DF30)]
        [InlineData(1000, 25, FrameRate.FPS25)]
        [InlineData(1000, 24, FrameRate.FPS24)]
        [InlineData(1000, 30, FrameRate.FPS30)]
        [InlineData(1000, 29, FrameRate.DF30)]
        public void MillisecondsToFrames_ReportsCorrectNumberOfFrames(double ms, int frames, FrameRate frameRate)
        {
            Assert.Equal(frames, ms.MillisecondsToFrames(frameRate));
        }

        [Theory]
        [Trait("TestType", "UnitTest")]
        [InlineData(0, 0, FrameRate.FPS25)]
        [InlineData(4, 160, FrameRate.FPS25)]
        [InlineData(4, 167, FrameRate.FPS24)]
        [InlineData(4, 133, FrameRate.FPS30)]
        [InlineData(4, 133, FrameRate.DF30)]
        [InlineData(25, 1000, FrameRate.FPS25)]
        [InlineData(24, 1000, FrameRate.FPS24)]
        [InlineData(30, 1000, FrameRate.FPS30)]
        [InlineData(30, 1001, FrameRate.DF30)]
        public void FramesToMilliseconds_ReportsCorrectNumberOfFrames(int frames, int milliseconds, FrameRate frameRate)
        {
            Assert.Equal(milliseconds, TimeUtilities.FramesToMiliseconds(frames, frameRate));
        }
    }
}
