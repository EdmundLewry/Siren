using System;
using System.Threading;
using System.Threading.Tasks;

namespace CBS.Siren.Device
{
    /*
    A Device is an abstraction for any device required to perform the playout of a list
    event. There can be many different kinds of devices, but each one will be provided with
    a Playout List, that they can then use to drive the functionality that they require.

    By updating the state of the Playout List Events, the device can provide feedback as well.
    */
    public interface IDevice : IDisposable
    {
        public enum DeviceStatus
        {
            STOPPED,
            PLAYING
        }

        DeviceModel Model { get; set;  }
        DeviceStatus CurrentStatus { get; }
        DeviceList ActiveList { get; set; }

        Task Run(CancellationToken token);
        event EventHandler<DeviceStatusEventArgs> OnDeviceStatusChanged;
        event EventHandler<DeviceListEventStatusChangeArgs> OnDeviceEventStatusChanged;
    }
}