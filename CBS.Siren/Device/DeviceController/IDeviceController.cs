using System;
using System.Threading;

namespace CBS.Siren.Device
{
    public interface IDeviceController
    {
        event EventHandler<DeviceEventChangedEventArgs> OnEventStarted;
        event EventHandler<DeviceEventChangedEventArgs> OnEventEnded;

        DeviceList ActiveDeviceList { get; set; }
        DeviceListEvent CurrentEvent { get; }

        void Run(CancellationToken token);
    }
}
