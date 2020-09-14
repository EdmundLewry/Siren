using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CBS.Siren.Time;
using Microsoft.Extensions.Logging;

namespace CBS.Siren.Device.Poseidon
{
    public class PoseidonDeviceController : IDeviceController
    {
        private readonly ILogger _logger;
        public event EventHandler<DeviceEventChangedEventArgs> OnEventCue = delegate { };
        public event EventHandler<DeviceEventChangedEventArgs> OnEventStart = delegate { };
        public event EventHandler<DeviceEventChangedEventArgs> OnEventEnd = delegate { };
        public event EventHandler<EventArgs> OnDeviceListEnded = delegate { };

        private readonly object _deviceListLock = new object();
        private DeviceList _activeDeviceList;
        public DeviceList ActiveDeviceList { 
            get => _activeDeviceList; 
            set {
                lock (_deviceListLock)
                {
                    _activeDeviceList = value;
                    _activeDeviceList.Events.ForEach(listEvent => OnEventCue?.Invoke(this, new DeviceEventChangedEventArgs(listEvent)) );
                }
                _logger.LogInformation($"Device List with {_activeDeviceList.Events.Count} events has been set");
            } 
        }
        public DeviceListEvent CurrentEvent { get; }

        public PoseidonDeviceController(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Run(CancellationToken token)
        {
            await Task.Delay(5); //Stops us thrashing the CPU
        }
    }
}