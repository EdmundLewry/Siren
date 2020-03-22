using System;

namespace CBS.Siren.Device
{
    public class DeviceEventChangedEventArgs : EventArgs
    {
        public DeviceListEvent AffectedEvent { get; private set; }

        public DeviceEventChangedEventArgs(DeviceListEvent listEvent)
        {
            AffectedEvent = listEvent;
        }
    }
}
