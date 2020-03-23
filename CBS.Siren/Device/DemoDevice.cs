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
        public DemoDevice(String name)
        {
            Name = name;
            Controller = new DeviceController();
        }

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

        private async Task DoRun(CancellationToken token)
        {
            await Controller.Run(token);
        }

    }
}