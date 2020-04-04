﻿using CBS.Siren.Device;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CBS.Siren
{
    /*
        A Transmission List Service contains a list of Transmission Events which reference PLaylist Events
        and a specific Device or set of devices that are expected to be used to carry out
        the event. This service contains the entire List, and has the responsibility
        of performing cold validation of the events against where they're expected to playout
        given the current configuration and status of devices in the Channel's playout chain
        configuration.

        It also has the responsibility of generating the Device Lists for each device, which will
        be used to actually control the devices.
     */
    public class TransmissionListService : ITransmissionListService
    {
        public ILogger<TransmissionListService> Logger { get; }
        public IScheduler Scheduler { get; private set; }
        public IDeviceListEventWatcher DeviceListEventWatcher { get; }

        private TransmissionList _transmissionList;
        public TransmissionList TransmissionList 
        { 
            get => _transmissionList; 
            set {
                UnsubscribeFromDeviceEventChanges(_transmissionList);
                _transmissionList = value;
                SubscribeToDeviceEvents(_transmissionList);
            } 
        }

        public TransmissionListService(IScheduler scheduler, IDeviceListEventWatcher deviceListEventWatcher, ILogger<TransmissionListService> logger)
        {
            Logger = logger;
            Scheduler = scheduler;
            DeviceListEventWatcher = deviceListEventWatcher;
        }

        public void OnDeviceListEventStatusChanged(Guid eventId, DeviceListEventState state)
        {
            Logger.LogWarning("OnDeviceListEventStatusChanged: Not implemented yet");
        }

        public void PlayTransmissionList()
        {
            Dictionary<IDevice, DeviceList> deviceLists = Scheduler.ScheduleTransmissionList(TransmissionList);

            DeliverDeviceLists(deviceLists);
        }

        private void DeliverDeviceLists(Dictionary<IDevice, DeviceList> deviceLists)
        {
            deviceLists.ToList().ForEach((pair) => pair.Key.ActiveList = pair.Value);
        }

        private void SubscribeToDeviceEvents(TransmissionList transmissionList)
        {
            if (transmissionList is null)
            {
                return;
            }

            HashSet<IDevice> devices = transmissionList.Events.SelectMany(listEvent => listEvent.EventFeatures.Select(feature => feature.Device)).ToHashSet();
            foreach (IDevice device in devices)
            {
                DeviceListEventWatcher.SubcsribeToDevice(this, device);
            }
        }

        private void UnsubscribeFromDeviceEventChanges(TransmissionList transmissionList)
        {
            if(transmissionList is null)
            {
                return;
            }

            HashSet<IDevice> devices = transmissionList.Events.SelectMany(listEvent => listEvent.EventFeatures.Select(feature => feature.Device)).ToHashSet();
            foreach(IDevice device in devices)
            {
                DeviceListEventWatcher.UnsubcsribeFromDevice(this, device);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue && disposing)
            {
                UnsubscribeFromDeviceEventChanges(_transmissionList);
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
