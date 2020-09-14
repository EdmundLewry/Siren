using System;
using System.Threading;
using System.Threading.Tasks;
using CBS.Siren.Device;

namespace CBS.Siren.Device
{
    public interface IDeviceDriver : IDisposable
    {
        Task CueEvent(DeviceListEvent Event);
        Task StartEvent(DeviceListEvent Event);
        Task EndEvent(DeviceListEvent Event);

        event EventHandler<DeviceListEvent> OnEventCued;
        event EventHandler<DeviceListEvent> OnEventStarted;
        event EventHandler<DeviceListEvent> OnEventEnded;
        event EventHandler<DeviceListEvent> OnCueError;
        event EventHandler<DeviceListEvent> OnStartError;
        event EventHandler<DeviceListEvent> OnEndError;
    }
}
