using System;

namespace CBS.Siren
{
    public interface IDeviceListEventStatusChangeListener
    {
        void OnDeviceListEventStatusChanged(Guid eventId, DeviceListEventState state);
    }
}