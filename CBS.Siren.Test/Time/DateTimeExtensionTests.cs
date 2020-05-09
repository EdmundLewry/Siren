using CBS.Siren.Time;
using System;
using Xunit;

namespace CBS.Siren.Test.Time
{
    public class DateTimeExtensionTests
    {
        [Theory]
        [Trait("TestType", "UnitTest")]
        [InlineData("2020-03-22T00:00:00:00", FrameRate.FPS25, "22/03/2020 00:00:00.000")]
        [InlineData("2020-03-22T00:00:10:05", FrameRate.FPS25, "22/03/2020 00:00:10.200")]
        [InlineData("2020-03-22T00:00:10:05", FrameRate.FPS30, "22/03/2020 00:00:10.167")]
        [InlineData("2020-03-22T12:30:10;05", FrameRate.DF30, "22/03/2020 12:30:10.167")]
        [InlineData("1980-12-15T22:20:45:05", FrameRate.FPS24, "15/12/1980 22:20:45.208")]
        [InlineData("2020-03-22", FrameRate.FPS25, "22/03/2020 00:00:00.000")]
        public void FromTimecodeString_GivenStringAndFramerate_ShouldReturnDateTime(string input, FrameRate frameRate, string expectedOutput)
        {
            DateTime valueUnderTest = DateTimeExtensions.FromTimecodeString(input, frameRate);
            DateTime expectedValue = DateTime.Parse(expectedOutput);
            Assert.Equal(expectedValue, valueUnderTest);
        }

        [Theory]
        [Trait("TestType", "UnitTest")]
        [InlineData("2020-03-38T00:00:00:00", FrameRate.FPS25)]
        [InlineData("2020-13-22T00:00:10:05", FrameRate.FPS30)]
        [InlineData("2020-03-22T12:30:10;40", FrameRate.DF30)]
        [InlineData("1980-12-15T25:20:45:05", FrameRate.FPS24)]
        [InlineData("1980-12-15T20:72:45:05", FrameRate.FPS24)]
        [InlineData("1980-12-15T20:20:75:05", FrameRate.FPS24)]
        [InlineData("2020-03-28T00:00:00:000", FrameRate.FPS25)]
        [InlineData("22020-03-28T05:10:00;00", FrameRate.DF30)]
        [InlineData("2020-03-28T00:00:00:00", FrameRate.DF30)]
        [InlineData("2020-03-22T", FrameRate.FPS25)]
        public void FromTimecodeString_GivenAnInvalidString_ShouldThrow(string input, FrameRate frameRate)
        {
            Assert.Throws<ArgumentException>(() => DateTimeExtensions.FromTimecodeString(input, frameRate));
        }

        [Theory]
        [Trait("TestType", "UnitTest")]
        [InlineData("22/03/2020 00:00:00.000", "2020-03-22T00:00:00:00", FrameRate.FPS25)]
        [InlineData("22/12/2020 00:00:10.000", "2020-12-22T00:00:10:00", FrameRate.FPS25)]
        [InlineData("22/12/2020 00:00:10.999", "2020-12-22T00:00:10:24", FrameRate.FPS25)]
        [InlineData("02/03/2020 00:00:00.960", "2020-03-02T00:00:00:24", FrameRate.FPS25)]
        [InlineData("22/03/1999 01:02:01.500", "1999-03-22T01:02:01:12", FrameRate.FPS24)]
        [InlineData("22/03/2020 01:02:01.867", "2020-03-22T01:02:01:26", FrameRate.FPS30)]
        [InlineData("22/03/2020 01:02:01.867", "2020-03-22T01:02:01;25", FrameRate.DF30)]
        [InlineData("22/03/2020 11:22:41.867", "2020-03-22T11:22:41;25", FrameRate.DF30)]
        public void ToTimecodeString_GivenTimeSpan_ReturnsExpectedString(string input, string expected, FrameRate frameRate)
        {
            string output = DateTime.Parse(input).ToTimecodeString(frameRate);
            Assert.Equal(expected, output);
        }
    }
}
