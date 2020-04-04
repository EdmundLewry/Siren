using Microsoft.Extensions.Logging;

namespace CBS.Siren.Device
{
    public interface IDeviceFactory
    {
        IDevice CreateDemoDevice(string name, ILoggerFactory loggerFactory);
    }
}
