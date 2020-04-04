using Microsoft.Extensions.Logging;

namespace CBS.Siren.Device
{
    public class DeviceFactory : IDeviceFactory
    {
        public IDevice CreateDemoDevice(string name, ILoggerFactory loggerFactory)
        {
            IDeviceController controller = new DeviceController(loggerFactory.CreateLogger<DeviceController>());
            IDeviceDriver deviceDriver = new DeviceDriver(controller, loggerFactory.CreateLogger<DeviceDriver>());
            return new DemoDevice(name, controller, deviceDriver);
        }
    }
}
