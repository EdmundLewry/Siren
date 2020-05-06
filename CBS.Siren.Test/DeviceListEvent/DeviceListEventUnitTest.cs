using CBS.Siren.Time;
using System;
using Xunit;

namespace CBS.Siren.Test
{
    public class DeviceListEventUnitTest
    {
        private string GenerateDeviceListEventData(DateTime startTime, DateTime endTime)
        {
            return $"{{\"timing\":{{\"startTime\":\"{startTime.ToTimecodeString()}\",\"duration\":\"00:00:01:00\",\"endTime\":\"{endTime.ToTimecodeString()}\"}}}}";
        }

        [Fact]
        [Trait("TestType","UnitTest")]
        public void OnCreation_DeviceListEvent_ShouldSetTimingData()
        {
            DateTime start = DateTime.Parse("12/02/2020 12:00:00");
            DateTime end = start.AddMinutes(2);
            string eventData = GenerateDeviceListEventData(start, end);
            
            DeviceListEvent deviceListEvent = new DeviceListEvent(eventData);
            Assert.Equal(start, deviceListEvent.StartTime);
            Assert.Equal(end, deviceListEvent.EndTime);
        }
    }
}
