using System;
using System.Threading;
using System.Threading.Tasks;
using CBS.Siren.Device;

namespace CBS.Siren.Device.Poseidon
{
    public class PoseidonDevice : IDevice
    {
        public DeviceModel Model { get; set; }

        public IDevice.DeviceStatus CurrentStatus { get; set; } = IDevice.DeviceStatus.STOPPED;

        public DeviceList ActiveList { get => Controller.ActiveDeviceList; set => Controller.ActiveDeviceList = value; }

        private IDeviceController Controller { get; set; }
        private IDeviceDriver Driver { get; set; }

        public event EventHandler<DeviceStatusEventArgs> OnDeviceStatusChanged = delegate { };
        public event EventHandler<DeviceListEventStatusChangeArgs> OnDeviceEventStatusChanged = delegate { };

        public PoseidonDevice(DeviceModel model, IDeviceController controller, IDeviceDriver driver)
        {
            Model = model;
            Controller = controller;
            Driver = driver;            
            SubscribeToControllerEvents();
            SubscribeToDriverEvents();
        }

        private void SubscribeToControllerEvents()
        {
            Controller.OnEventCue += (s, e) => Driver.CueEvent(e.AffectedEvent);
            Controller.OnEventStart += (s, e) => Driver.StartEvent(e.AffectedEvent);
            Controller.OnEventEnd += (s, e) => Driver.EndEvent(e.AffectedEvent);
            //Controller.OnDeviceListEnded += DeviceListEndEventHandler;
        }

        private void UnsubscribeFromControllerEvents()
        {
            //Controller.OnEventStart -= DeviceEventChangeEventHandler;
            //Controller.OnEventEnd -= DeviceEventChangeEventHandler;
            //Controller.OnDeviceListEnded -= DeviceListEndEventHandler;
        }

        private void SubscribeToDriverEvents()
        {

        }

        public async Task Run(CancellationToken token)
        {
            await Controller.Run(token);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    UnsubscribeFromControllerEvents();
                    Driver.Dispose();
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