using System;
using System.Threading;
using System.Threading.Tasks;

namespace CBS.Siren.Device
{
    /*
    The Demo Device is an implementation of the Device interface that allows us to simulate how
    the Automation Domain will interact with real devices.
    */
    public class DemoDevice : IDevice
    {
        public event EventHandler<DeviceStatusEventArgs> OnDeviceStatusChanged = delegate { };
        public event EventHandler<DeviceListEventStatusChangeArgs> OnDeviceEventStatusChanged = delegate { };

        private IDeviceController Controller { get; set; }
        private IDeviceDriver Driver { get; set; }

        private EventHandler<DeviceEventChangedEventArgs> DeviceEventChangeEventHandler { get; set; }
        private EventHandler<EventArgs> DeviceListEndEventHandler { get; set; }

        public DeviceModel Model { get; set; }

        public IDevice.DeviceStatus CurrentStatus { get; set; } = IDevice.DeviceStatus.STOPPED;

        public DeviceList ActiveList { get => Controller.ActiveDeviceList; set => Controller.ActiveDeviceList = value; }


        public DemoDevice(DeviceModel model, IDeviceController controller, IDeviceDriver driver)
        {
            Model = model;
            Controller = controller;
            Driver = driver;

            DeviceEventChangeEventHandler = new EventHandler<DeviceEventChangedEventArgs>((sender, args) => HandleDeviceEventChange(args));
            DeviceListEndEventHandler = new EventHandler<EventArgs>((sender, args) => AssessDeviceStatus());
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
            Controller.OnEventStart -= DeviceEventChangeEventHandler;
            Controller.OnEventEnd -= DeviceEventChangeEventHandler;
            Controller.OnDeviceListEnded -= DeviceListEndEventHandler;
        }

        private void SubscribeToDriverEvents()
        {
            //Driver.OnEventCued += (s, e) => 
        }

        private void UnsubscribeFromDriverEvents()
        {

        }

        public override String ToString()
        {
            return base.ToString() + " Name: " + Model.Name;
        }

        public async Task Run(CancellationToken token)
        {
            await Controller.Run(token);
        }

        private void HandleDeviceEventChange(DeviceEventChangedEventArgs args)
        {
            OnDeviceEventStatusChanged?.Invoke(this, new DeviceListEventStatusChangeArgs(args.AffectedEvent.Id, args.AffectedEvent.EventState));
            AssessDeviceStatus();
        }

        private void AssessDeviceStatus()
        {
            if (Controller.CurrentEvent != null && CurrentStatus == IDevice.DeviceStatus.STOPPED)
            {
                ChangeStatus(IDevice.DeviceStatus.PLAYING);

            }

            if (Controller.CurrentEvent == null && CurrentStatus == IDevice.DeviceStatus.PLAYING)
            {
                ChangeStatus(IDevice.DeviceStatus.STOPPED);
            }
        }

        private void ChangeStatus(IDevice.DeviceStatus newStatus)
        {
            IDevice.DeviceStatus oldStatus = CurrentStatus;
            CurrentStatus = newStatus;
            OnDeviceStatusChanged?.Invoke(this, new DeviceStatusEventArgs(oldStatus, CurrentStatus));
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
                    UnsubscribeFromDriverEvents();
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