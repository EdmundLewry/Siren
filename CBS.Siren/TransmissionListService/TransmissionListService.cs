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
        public IDeviceListEventStore DeviceListEventStore { get; }

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

        public TransmissionListService(IScheduler scheduler, IDeviceListEventWatcher deviceListEventWatcher, IDeviceListEventStore deviceListEventStore, ILogger<TransmissionListService> logger)
        {
            Logger = logger;
            Scheduler = scheduler;
            DeviceListEventWatcher = deviceListEventWatcher;
            DeviceListEventStore = deviceListEventStore;
        }

        public void OnDeviceListEventStatusChanged(int eventId, int? transmissionListEventId, DeviceListEventState state)
        {
            //Find the transmission list event that relates to this id
            TransmissionListEvent effectedEvent = transmissionListEventId.HasValue ? GetTransmissionListEventById(transmissionListEventId.Value) : FindTransmissionListEventByDeviceListEventId(eventId);

            switch (state.CurrentStatus)
            {
                case DeviceListEventState.Status.CUED:
                    {
                        if (IsTransmissionListEventCued(effectedEvent, DeviceListEventStore))
                        {
                            UpdateTransmissionListEventStatus(effectedEvent, TransmissionListEventState.Status.CUED);
                            break;
                        }

                        UpdateTransmissionListEventStatus(effectedEvent, TransmissionListEventState.Status.CUEING);
                        break;
                    }
                case DeviceListEventState.Status.PLAYING:
                    {
                        UpdateTransmissionListEventStatus(effectedEvent, TransmissionListEventState.Status.PLAYING);
                        break;
                    }
                case DeviceListEventState.Status.PLAYED:
                    {
                        if(IsTransmissionListEventPlayed(effectedEvent, DeviceListEventStore))
                        {
                            UpdateTransmissionListEventStatus(effectedEvent, TransmissionListEventState.Status.PLAYED);
                        }
                        break;
                    }

            }
        }

        private bool AreAllFeatureDeviceEventsInState(TransmissionListEvent effectedEvent, IDeviceListEventStore deviceListEventStore, DeviceListEventState.Status targetState)
        {
            IEnumerable<int> deviceListEvents = effectedEvent.EventFeatures.Where(feature => feature.DeviceListEventId.HasValue).Select(feature => feature.DeviceListEventId.Value);
            return deviceListEvents.All(deviceListEventId => deviceListEventStore.GetEventById(deviceListEventId)?.EventState.CurrentStatus == targetState);
        }

        private bool IsTransmissionListEventCued(TransmissionListEvent effectedEvent, IDeviceListEventStore deviceListEventStore) => 
            AreAllFeatureDeviceEventsInState(effectedEvent, deviceListEventStore, DeviceListEventState.Status.CUED);

        private bool IsTransmissionListEventPlayed(TransmissionListEvent effectedEvent, IDeviceListEventStore deviceListEventStore) =>
            AreAllFeatureDeviceEventsInState(effectedEvent, deviceListEventStore, DeviceListEventState.Status.PLAYED);


        private void UpdateTransmissionListEventStatus(TransmissionListEvent effectedEvent, TransmissionListEventState.Status status)
        {
            if(effectedEvent.EventState.CurrentStatus != status)
            {
                effectedEvent.EventState.CurrentStatus = status;
            }
        }

        public TransmissionListEvent GetTransmissionListEventById(int transmissionListEventId)
        {
            return TransmissionList.Events.FirstOrDefault(listEvent => listEvent.Id == transmissionListEventId);
        }

        private TransmissionListEvent FindTransmissionListEventByDeviceListEventId(int deviceListEventId)
        {
            return TransmissionList.Events.FirstOrDefault(listEvent => listEvent.EventFeatures.Select(feature => feature.DeviceListEventId).Contains(deviceListEventId));
        }

        public void PlayTransmissionList()
        {
            Dictionary<IDevice, DeviceList> deviceLists = Scheduler.ScheduleTransmissionList(TransmissionList, DeviceListEventStore);

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
                if(device is null)
                {
                    continue;
                }
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
                if(device is null)
                {
                    continue;
                }
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
