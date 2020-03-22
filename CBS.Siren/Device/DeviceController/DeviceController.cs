using System;
using System.Threading;

namespace CBS.Siren.Device
{
    public class DeviceController : IDeviceController
    {
        public event EventHandler<DeviceEventChangedEventArgs> OnEventStarted = delegate { };
        public event EventHandler<DeviceEventChangedEventArgs> OnEventEnded = delegate { };
        
        public DeviceList ActiveDeviceList { get; set; }

        public DeviceListEvent CurrentEvent => throw new NotImplementedException();

        public DeviceController()
        {

        }

        public void Run(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
