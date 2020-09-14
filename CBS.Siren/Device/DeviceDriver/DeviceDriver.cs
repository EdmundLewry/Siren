using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CBS.Siren.Device
{
    public class DeviceDriver : IDeviceDriver
    {
        private IDeviceController Controller { get; set; }
        private ILogger Logger { get; set; }

        public event EventHandler<DeviceListEvent> OnEventCued = delegate { };     
        public event EventHandler<DeviceListEvent> OnEventStarted = delegate { }; 
        public event EventHandler<DeviceListEvent> OnEventEnded = delegate { }; 

        public event EventHandler<DeviceListEvent> OnCueError = delegate { }; 
        public event EventHandler<DeviceListEvent> OnStartError = delegate { }; 
        public event EventHandler<DeviceListEvent> OnEndError = delegate { };    

        public DeviceDriver(IDeviceController controller, ILogger logger)
        {
            Controller = controller;
            Logger = logger;
        }

        public async Task CueEvent(DeviceListEvent Event)
        {
            await Task.Delay(1000);
            Logger.LogInformation($"Event Cued: {Event.ToString()}");
            Event.EventState.CurrentStatus = DeviceListEventState.Status.CUED;
            OnEventCued?.Invoke(this, Event);
        }

        public async Task StartEvent(DeviceListEvent Event)
        {
            await Task.Delay(5);
            Logger.LogInformation($"Event Started: {Event.ToString()}");
            Event.EventState.CurrentStatus = DeviceListEventState.Status.PLAYING;
            OnEventStarted?.Invoke(this, Event);
        }

        public async Task EndEvent(DeviceListEvent Event)
        {
            await Task.Delay(5);
            Logger.LogInformation($"Event Ended: {Event.ToString()}");
            Event.EventState.CurrentStatus = DeviceListEventState.Status.PLAYED;
            OnEventEnded?.Invoke(this, Event);
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
