using CBS.Siren.Time;
using System;
using Xunit;

namespace CBS.Siren.Test
{
    public class DeviceListEventUnitTest
    {
        private string GenerateDeviceListEventData(DateTimeOffset startTime, DateTimeOffset endTime)
        {
            return $"{{\"timing\":{{\"startTime\":\"{startTime.ToTimecodeString()}\",\"duration\":\"00:00:01:00\",\"endTime\":\"{endTime.ToTimecodeString()}\"}}}}";
        }

        [Fact]
        [Trait("TestType","UnitTest")]
        public void OnCreation_DeviceListEvent_ShouldSetTimingData()
        {
            DateTimeOffset start = DateTimeOffset.Parse("12/02/2020 12:00:00");
            DateTimeOffset end = start.AddMinutes(2);
            string eventData = GenerateDeviceListEventData(start, end);
            
            DeviceListEvent deviceListEvent = new DeviceListEvent(eventData);
            Assert.Equal(start, deviceListEvent.StartTime);
            Assert.Equal(end, deviceListEvent.EndTime);
        }
    }
}
