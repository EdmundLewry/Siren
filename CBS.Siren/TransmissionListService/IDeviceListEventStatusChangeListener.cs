﻿using System;

namespace CBS.Siren
{
    public interface IDeviceListEventStatusChangeListener : IDisposable
    {
        void OnDeviceListEventStatusChanged(int eventId, DeviceListEventState state);
    }
}