using System;

namespace CBS.Siren
{
    public class DeviceListEventState
    {
        public DeviceListEventStatus CurrentStatus { get; set; } = DeviceListEventStatus.UNSCHEDULED;

        public override string ToString()
        {
            return Enum.GetName(typeof(DeviceListEventStatus), CurrentStatus);
        }
    }
}