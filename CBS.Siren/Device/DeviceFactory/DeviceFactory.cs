using Microsoft.Extensions.Logging;
using CBS.Siren.Device.Poseidon;
using System.Net.Http;

namespace CBS.Siren.Device
{
    public class DeviceFactory : IDeviceFactory
    {
        public IDevice CreateDemoDevice(DeviceModel model, ILoggerFactory loggerFactory)
        {
            IDeviceController controller = new DeviceController(loggerFactory.CreateLogger<DeviceController>());
            IDeviceDriver deviceDriver = new DeviceDriver(controller, loggerFactory.CreateLogger<DeviceDriver>());
            return new DemoDevice(model, controller, deviceDriver);
        }

        public IDevice CreatePoseidonDevice(DeviceModel model, ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
        {
            IDeviceController controller = new PoseidonDeviceController(loggerFactory.CreateLogger<PoseidonDeviceController>());
            IDeviceDriver deviceDriver = new PoseidonDeviceDriver(loggerFactory.CreateLogger<PoseidonDeviceDriver>(), httpClientFactory);
            return new DemoDevice(model, controller, deviceDriver);
        }
    }
}
