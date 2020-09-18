using Microsoft.Extensions.Logging;

namespace CBS.Siren.Device
{
    public interface IDeviceFactory
    {
        IDevice CreateDemoDevice(DeviceModel model, ILoggerFactory loggerFactory, IDeviceListEventStore deviceListEventStore);
    }
}
