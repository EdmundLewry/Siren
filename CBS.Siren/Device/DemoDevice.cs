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

        public String Name { get; }

        public IDevice.DeviceStatus CurrentStatus { get; set; } = IDevice.DeviceStatus.STOPPED;

        public DeviceList ActiveList { get => Controller.ActiveDeviceList; set => Controller.ActiveDeviceList = value; }


        public DemoDevice(String name, IDeviceController controller, IDeviceDriver driver)
        {
            Name = name;
            Controller = controller;
            Driver = driver;
            DeviceEventChangeEventHandler = new EventHandler<DeviceEventChangedEventArgs>((sender, args) => HandleDeviceEventChange(args));
            DeviceListEndEventHandler = new EventHandler<EventArgs>((sender, args) => AssessDeviceStatus());
            SubscribeToControllerEvents();
        }

        private void SubscribeToControllerEvents()
        {
            Controller.OnEventStarted += DeviceEventChangeEventHandler;
            Controller.OnEventEnded += DeviceEventChangeEventHandler;
            Controller.OnDeviceListEnded += DeviceListEndEventHandler;
        }

        private void UnsubscribeFromControllerEvents()
        {
            Controller.OnEventStarted -= DeviceEventChangeEventHandler;
            Controller.OnEventEnded -= DeviceEventChangeEventHandler;
            Controller.OnDeviceListEnded -= DeviceListEndEventHandler;
        }


        public override String ToString()
        {
            return base.ToString() + " Name: " + Name;
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