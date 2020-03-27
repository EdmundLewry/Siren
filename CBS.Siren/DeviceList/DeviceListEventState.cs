using System;

namespace CBS.Siren
{
    public class DeviceListEventState
    {
        public enum Status
        {
            UNSCHEDULED,
            CUED,
            PLAYING,
            PLAYED
        }

        public Status CurrentStatus { get; set; } = Status.UNSCHEDULED;

        public override string ToString()
        {
            return Enum.GetName(typeof(Status), CurrentStatus);
        }
    }
}