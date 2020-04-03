using CBS.Siren.Device;

namespace CBS.Siren
{
    /* The DeviceListEventWatcher is an abstraction for retrieving and passing on change events
     * from Devices for Device List Events. 
     * 
     * With this abstraction, we can easily change the source of the DeviceListEvent Status Change events from
     * the local device objects to updates from a REST API when the Devices are separated out eventually.
     * 
     */
    public interface IDeviceListEventWatcher
    {
        void SubcsribeToDevice(IDeviceListEventStatusChangeListener listener, IDevice device);
        void UnsubcsribeFromDevice(IDeviceListEventStatusChangeListener listener, IDevice device);
    }
}
