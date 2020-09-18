using Microsoft.Extensions.Logging;

namespace CBS.Siren.Device
{
    public class DeviceFactory : IDeviceFactory
    {
        public IDevice CreateDemoDevice(DeviceModel model, ILoggerFactory loggerFactory, IDeviceListEventStore deviceListEventStore)
        {
            IDeviceController controller = new DeviceController(loggerFactory.CreateLogger<DeviceController>(), deviceListEventStore);
            IDeviceDriver deviceDriver = new DeviceDriver(controller, loggerFactory.CreateLogger<DeviceDriver>());
            return new DemoDevice(model, controller, deviceDriver);
        }
    }
}
