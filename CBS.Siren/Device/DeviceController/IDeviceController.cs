using System;
using System.Threading;
using System.Threading.Tasks;

namespace CBS.Siren.Device
{
    public interface IDeviceController
    {
        event EventHandler<DeviceEventChangedEventArgs> OnEventStarted;
        event EventHandler<DeviceEventChangedEventArgs> OnEventEnded;

        DeviceList ActiveDeviceList { get; set; }
        DeviceListEvent CurrentEvent { get; }

        Task Run(CancellationToken token);
    }
}
