using CBS.Siren.Time;
using System;
using Xunit;

namespace CBS.Siren.Test.Time
{
    public class TimespanExtensionTests
    {
        [Theory]
        [Trait("TestType", "UnitTest")]
        [InlineData("00:00:00:00", FrameRate.FPS25, 0)]
        [InlineData("00:00:10:05", FrameRate.FPS25, 10200)]
        [InlineData("00:00:10:05", FrameRate.FPS30, 10167)]
        [InlineData("00:00:10;05", FrameRate.DF30, 10167)]
        [InlineData("00:00:10:05", FrameRate.FPS24, 10208)]
        [InlineData("00:00:10;26", FrameRate.DF30, 10868)]
        [InlineData("00:00:10:26", FrameRate.FPS30, 10867)]
        [InlineData("01:01:20:15", FrameRate.FPS25, 3680600)]
        [InlineData("001:01:03:00:05", FrameRate.FPS25, 90180200)]
        public void FromTimecodeString_GivenStringAndFramerate_ShouldReturnTimespan(string input, FrameRate framerate, int expectedOutput)
        {
            TimeSpan valueUnderTest = TimeSpanExtensions.FromTimecodeString(input, framerate);
            Assert.Equal(expectedOutput, valueUnderTest.TotalMilliseconds);
        }

        [Theory]
        [Trait("TestType", "UnitTest")]
        [InlineData("00:00:00:000", FrameRate.FPS25)]
        [InlineData("00:00:00:30", FrameRate.FPS25)]
        [InlineData("00:00:00;20", FrameRate.FPS24)]
        [InlineData("00:00:00:20", FrameRate.DF30)]
        [InlineData("00:00:00:25", FrameRate.FPS24)]
        [InlineData("00:00:00:aa", FrameRate.FPS24)]
        [InlineData("00:00:00:32", FrameRate.FPS30)]
        [InlineData("00:00:72:00", FrameRate.FPS25)]
        [InlineData("00:72:00:00", FrameRate.FPS25)]
        [InlineData("26:00:00:00", FrameRate.FPS25)]
        public void FromTimecodeString_GivenInvalidStringFormat_ShouldThrowException(string input, FrameRate framerate)
        {
            Assert.Throws<ArgumentException>(() => TimeSpanExtensions.FromTimecodeString(input, framerate));
        }

    }
}
