using Microsoft.Extensions.Logging;
using System;

namespace CBS.Siren
{
    public class TransmissionListService : ITransmissionListService
    {
        public ILogger<TransmissionListService> Logger { get; }

        public TransmissionListService(ILogger<TransmissionListService> logger)
        {
            Logger = logger;
        }

        public void OnDeviceListEventStatusChanged(Guid eventId, DeviceListEventState state)
        {
            Logger.LogWarning("OnDeviceListEventStatusChanged: Not implemented yet");
        }
    }
}
