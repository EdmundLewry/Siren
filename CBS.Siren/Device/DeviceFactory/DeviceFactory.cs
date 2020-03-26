using Microsoft.Extensions.Logging;

namespace CBS.Siren.Device
{
    public class DeviceFactory : IDeviceFactory
    {
        public IDevice CreateDemoDevice(string name, ILogger logger)
        {
            IDeviceController controller = new DeviceController(logger);
            IDeviceDriver deviceDriver = new DeviceDriver(controller, logger);
            return new DemoDevice(name, controller, deviceDriver);
        }
    }
}
