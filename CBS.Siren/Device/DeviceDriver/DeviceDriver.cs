using Microsoft.Extensions.Logging;
using System;

namespace CBS.Siren.Device
{
    public class DeviceDriver : IDisposable, IDeviceDriver
    {
        private IDeviceController Controller { get; set; }
        private ILogger _logger { get; set; }

        private readonly EventHandler<DeviceEventChangedEventArgs> DeviceEventStart;
        private readonly EventHandler<DeviceEventChangedEventArgs> DeviceEventEnd;

        public DeviceDriver(IDeviceController controller, ILogger logger)
        {
            Controller = controller;
            _logger = logger;
            DeviceEventStart = new EventHandler<DeviceEventChangedEventArgs>((sender, args) => _logger.LogInformation($"Demo Device Playing: {args.AffectedEvent.ToString()}"));
            DeviceEventEnd = new EventHandler<DeviceEventChangedEventArgs>((sender, args) => _logger.LogInformation($"Demo Device Stopped Playing: {args.AffectedEvent.ToString()}"));
            Controller.OnEventStarted += DeviceEventStart;
            Controller.OnEventEnded += DeviceEventEnd;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if(Controller != null)
                    {
                        Controller.OnEventStarted -= DeviceEventStart;
                        Controller.OnEventEnded -= DeviceEventEnd;
                    }
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
