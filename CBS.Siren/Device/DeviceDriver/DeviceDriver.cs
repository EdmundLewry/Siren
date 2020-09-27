using Microsoft.Extensions.Logging;
using System;

namespace CBS.Siren.Device
{
    public class DeviceDriver : IDeviceDriver
    {
        private IDeviceController Controller { get; set; }
        private ILogger Logger { get; set; }

        private readonly EventHandler<DeviceEventChangedEventArgs> _deviceEventStart;
        private readonly EventHandler<DeviceEventChangedEventArgs> _deviceEventEnd;

        public DeviceDriver(IDeviceController controller, ILogger logger)
        {
            Controller = controller;
            Logger = logger;
            _deviceEventStart = new EventHandler<DeviceEventChangedEventArgs>((sender, args) => Logger.LogInformation($"Demo Device Playing:\n{args.AffectedEvent}\n"));
            _deviceEventEnd = new EventHandler<DeviceEventChangedEventArgs>((sender, args) => Logger.LogInformation($"Demo Device Stopped Playing:\n{args.AffectedEvent}\n"));
            Controller.OnEventStarted += _deviceEventStart;
            Controller.OnEventEnded += _deviceEventEnd;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && Controller != null)
                {
                    Controller.OnEventStarted -= _deviceEventStart;
                    Controller.OnEventEnded -= _deviceEventEnd;
                }

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
