using CBS.Siren.Device;
using System;
using System.Collections.Generic;

namespace CBS.Siren
{
    public class DeviceListEventWatcher : IDeviceListEventWatcher
    {
        public EventHandler<DeviceListEventStatusChangeArgs> EventStatusChangeHandler { get; set; }
        private Dictionary<IDevice, List<IDeviceListEventStatusChangeListener>> Subscriptions { get; set; }

        public DeviceListEventWatcher()
        {
            EventStatusChangeHandler = new EventHandler<DeviceListEventStatusChangeArgs>((sender, args) => OnDeviceListEventStatusChange(sender, args));
        }

        public void SubcsribeToDevice(IDeviceListEventStatusChangeListener listener, IDevice device)
        {
            //If no active subscription for device, subscribe to device for event status changes
            //if active subscription, add listener to list
            throw new NotImplementedException();
        }

        public void UnsubcsribeFromDevice(IDeviceListEventStatusChangeListener listener, IDevice device)
        {
            //Remove listener from active subscription
            //if no more listeners, unscubscribe from device
            throw new NotImplementedException();
        }

        public void OnDeviceListEventStatusChange(object sender, DeviceListEventStatusChangeArgs args)
        {
            //use as IDevice
        }
    }
}
