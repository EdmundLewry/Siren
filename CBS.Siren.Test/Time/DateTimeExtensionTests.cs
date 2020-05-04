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
    }
}
