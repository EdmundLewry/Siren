using System;

namespace CBS.Siren
{
    public interface IDeviceListEventStatusChangeListener : IDisposable
    {
        void OnDeviceListEventStatusChanged(Guid eventId, DeviceListEventState state);
    }
}