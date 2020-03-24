using Microsoft.Extensions.Logging;
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
        private IDeviceController Controller { get; set; }
        private DeviceDriver Driver { get; set; }

        public String Name { get; }

        public IDevice.DeviceStatus CurrentStatus { get; set; } = IDevice.DeviceStatus.STOPPED;
        public DemoDevice(String name, ILogger logger)
        {
            Name = name;
            Controller = new DeviceController(logger);
            Driver = new DeviceDriver(Controller, logger);
        }

        public event EventHandler<DeviceStatusEventArgs> OnDeviceStatusChanged = delegate { };

        public override String ToString()
        {
            return base.ToString() + " Name: " + Name;
        }

        public void SetDeviceList(DeviceList deviceList)
        {
            Controller.ActiveDeviceList = deviceList;
        }

        public void Run(CancellationToken token)
        {
            DoRun(token).Wait();
        }

        //TODO: Unit test this
        private async Task DoRun(CancellationToken token)
        {
            Task controllerTask = Controller.Run(token);

            while(!token.IsCancellationRequested)
            {
                if(Controller.CurrentEvent != null && CurrentStatus == IDevice.DeviceStatus.STOPPED)
                {
                    ChangeStatus(IDevice.DeviceStatus.PLAYING);
                    
                }

                if(Controller.CurrentEvent == null && CurrentStatus == IDevice.DeviceStatus.PLAYING)
                {
                    ChangeStatus(IDevice.DeviceStatus.STOPPED);
                }
            }

            await controllerTask;
        }

        private void ChangeStatus(IDevice.DeviceStatus newStatus)
        {
            IDevice.DeviceStatus oldStatus = CurrentStatus;
            CurrentStatus = newStatus;
            OnDeviceStatusChanged?.Invoke(this, new DeviceStatusEventArgs(oldStatus, CurrentStatus));
        }
    }
}