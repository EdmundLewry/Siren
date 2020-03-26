using System;
using Xunit;

namespace CBS.Siren.Test
{
    public class DeviceListEventUnitTest
    {
        private string GenerateDeviceListEventData(DateTime startTime, DateTime endTime)
        {
            return $"{{\"timing\":{{\"startTime\":\"{startTime.ToString("o")}\",\"duration\":25,\"endTime\":\"{endTime.ToString("o")}\"}}}}";
        }

        [Fact]
        [Trait("TestType","UnitTest")]
        public void OnCreation_DeviceListEvent_ShouldSetTimingData()
        {
            DateTime start = DateTime.Now;
            DateTime end = start.AddMinutes(2);
            string eventData = GenerateDeviceListEventData(start, end);
            
            DeviceListEvent deviceListEvent = new DeviceListEvent(eventData);
            Assert.Equal(start, deviceListEvent.StartTime);
            Assert.Equal(end, deviceListEvent.EndTime);
        }
    }
}
