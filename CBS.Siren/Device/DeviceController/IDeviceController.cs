using System;
using System.Threading;
using System.Threading.Tasks;

namespace CBS.Siren.Device
{
    public interface IDeviceController
    {
        event EventHandler<DeviceEventChangedEventArgs> OnEventCue;
        event EventHandler<DeviceEventChangedEventArgs> OnEventStart;
        event EventHandler<DeviceEventChangedEventArgs> OnEventEnd;
        event EventHandler<EventArgs> OnDeviceListEnded;

        DeviceList ActiveDeviceList { get; set; }
        DeviceListEvent CurrentEvent { get; }

        Task Run(CancellationToken token);
    }
}
