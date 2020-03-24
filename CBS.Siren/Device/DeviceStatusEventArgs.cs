namespace CBS.Siren.Device
{
    public class DeviceStatusEventArgs
    {
        public IDevice.DeviceStatus NewStatus { get; set; }
        public IDevice.DeviceStatus OldStatus { get; set; }
        public DeviceStatusEventArgs(IDevice.DeviceStatus oldStatus, IDevice.DeviceStatus newStatus)
        {
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }
}
