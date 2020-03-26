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
        private IDeviceDriver Driver { get; set; }

        public String Name { get; }

        public IDevice.DeviceStatus CurrentStatus { get; set; } = IDevice.DeviceStatus.STOPPED;
        public DemoDevice(String name, IDeviceController controller, IDeviceDriver driver)
        {
            Name = name;
            Controller = controller;
            Driver = driver;
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

        public async Task Run(CancellationToken token)
        {
            await DoRun(token);
        }

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

                await Task.Delay(5); //This just prevents us from thrashing the CPU
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