using CBS.Siren.Device;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CBS.Siren
{
    public class DeviceListEventWatcher : IDeviceListEventWatcher
    {
        public EventHandler<DeviceListEventStatusChangeArgs> EventStatusChangeHandler { get; private set; }
        private Dictionary<IDevice, List<IDeviceListEventStatusChangeListener>> Subscriptions { get; set; } = new Dictionary<IDevice, List<IDeviceListEventStatusChangeListener>>();

        public DeviceListEventWatcher()
        {
            EventStatusChangeHandler = new EventHandler<DeviceListEventStatusChangeArgs>((sender, args) => OnDeviceListEventStatusChange(sender as IDevice, args));
        }

        public void SubcsribeToDevice(IDeviceListEventStatusChangeListener listener, IDevice device)
        {
            if(!Subscriptions.ContainsKey(device))
            {
                Subscriptions.Add(device, new List<IDeviceListEventStatusChangeListener>());
                device.OnDeviceEventStatusChanged += EventStatusChangeHandler;
            }

            Subscriptions[device].Add(listener);
        }

        public void UnsubcsribeFromDevice(IDeviceListEventStatusChangeListener listener, IDevice device)
        {
            if(!Subscriptions.ContainsKey(device))
            {
                return;
            }

            Subscriptions[device].Remove(listener);

            if(!Subscriptions[device].Any())
            {
                device.OnDeviceEventStatusChanged -= EventStatusChangeHandler;
                Subscriptions.Remove(device);
            }
        }

        public void OnDeviceListEventStatusChange(IDevice device, DeviceListEventStatusChangeArgs args)
        {
            if (device is null || !Subscriptions.ContainsKey(device))
            {
                return;
            }

            Subscriptions[device].ForEach((listener) => listener.OnDeviceListEventStatusChanged(args.EventId, args.RelatedTransmissionListEventId, args.NewState));
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue && disposing)
            {
                Subscriptions.Keys.ToList().ForEach((device) => device.OnDeviceEventStatusChanged -= EventStatusChangeHandler);

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
