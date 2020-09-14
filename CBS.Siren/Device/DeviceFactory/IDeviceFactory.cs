using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace CBS.Siren.Device
{
    public interface IDeviceFactory
    {
        IDevice CreateDemoDevice(DeviceModel model, ILoggerFactory loggerFactory);
        IDevice CreatePoseidonDevice(DeviceModel model, ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory);
    }
}
