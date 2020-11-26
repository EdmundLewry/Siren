using CBS.Siren.Device;
using CBS.Siren.Time;
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
        private HashSet<IDevice> SubscribedDevices { get; set; } = new HashSet<IDevice>();
        public ITimeSourceProvider Clock { get; set; } = TimeSource.TimeProvider;

        private TransmissionList _transmissionList;
        public TransmissionList TransmissionList 
        { 
            get => _transmissionList; 
            set {
                UnsubscribeFromAllDeviceEventChanges(_transmissionList);
                _transmissionList = value;
                UpdateDeviceSubscriptions();
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
                case DeviceListEventStatus.CUED:
                    {
                        if (IsTransmissionListEventCued(effectedEvent, DeviceListEventStore))
                        {
                            UpdateTransmissionListEventStatus(effectedEvent, TransmissionListEventState.Status.CUED);
                            break;
                        }

                        UpdateTransmissionListEventStatus(effectedEvent, TransmissionListEventState.Status.CUEING);
                        break;
                    }
                case DeviceListEventStatus.PLAYING:
                    {
                        if(effectedEvent.EventState.CurrentStatus != TransmissionListEventState.Status.PLAYING)
                        {
                            OnTransmissionListEventStartedPlaying(effectedEvent);
                        }
                        break;
                    }
                case DeviceListEventStatus.PLAYED:
                    {
                        if(IsTransmissionListEventPlayed(effectedEvent, DeviceListEventStore))
                        {
                            OnTransmissionListEventPlayedOutSuccessfully(effectedEvent);
                        }
                        break;
                    }
            }
        }

        private void OnTransmissionListEventStartedPlaying(TransmissionListEvent effectedEvent)
        {
            UpdateTransmissionListEventStatus(effectedEvent, TransmissionListEventState.Status.PLAYING);
            if (!effectedEvent.ActualStartTime.HasValue)
            {
                effectedEvent.ActualStartTime = Clock.Now;
            }
            TransmissionList.CurrentEventId = effectedEvent.Id;
        }

        private void OnTransmissionListEventPlayedOutSuccessfully(TransmissionListEvent affectedEvent)
        {
            SetEventAsPlayed(affectedEvent);
            if(_transmissionList.Events.All((listEvent) => listEvent.EventState.CurrentStatus == TransmissionListEventState.Status.PLAYED))
            {
                TransmissionList.State = TransmissionListState.Stopped;
            }
        }

        private void SetEventAsPlayed(TransmissionListEvent affectedEvent)
        {
            UpdateTransmissionListEventStatus(affectedEvent, TransmissionListEventState.Status.PLAYED);
            affectedEvent.ActualEndTime = Clock.Now;
        }
        
        private void OnTransmissionListEventReset(TransmissionListEvent effectedEvent)
        {
            UpdateTransmissionListEventStatus(effectedEvent, TransmissionListEventState.Status.SCHEDULED);
            effectedEvent.ActualStartTime = null;
        }

        private bool AreAllFeatureDeviceEventsInState(TransmissionListEvent effectedEvent, IDeviceListEventStore deviceListEventStore, DeviceListEventStatus targetState)
        {
            IEnumerable<int> deviceListEvents = effectedEvent.EventFeatures.Where(feature => feature.DeviceListEventId.HasValue).Select(feature => feature.DeviceListEventId.Value);
            return deviceListEvents.All(deviceListEventId => deviceListEventStore.GetEventById(deviceListEventId)?.EventState.CurrentStatus == targetState);
        }

        private bool IsTransmissionListEventCued(TransmissionListEvent effectedEvent, IDeviceListEventStore deviceListEventStore) => 
            AreAllFeatureDeviceEventsInState(effectedEvent, deviceListEventStore, DeviceListEventStatus.CUED);

        private bool IsTransmissionListEventPlayed(TransmissionListEvent effectedEvent, IDeviceListEventStore deviceListEventStore) =>
            AreAllFeatureDeviceEventsInState(effectedEvent, deviceListEventStore, DeviceListEventStatus.PLAYED);


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
            if(TransmissionList.State == TransmissionListState.Playing)
            {
                Logger.LogDebug("Transmission List is already playing. Play request ignored");
                return;
            }

            int startIndex = GetCurrentEventIndex();
            if(TransmissionList.Events[startIndex].EventState.CurrentStatus == TransmissionListEventState.Status.PLAYED)
            {
                ++startIndex;
                if(TransmissionList.Events.Count <= startIndex)
                {
                    Logger.LogDebug("Current event has completed playing out and there's no subsequent event. Play request ignored.");
                    return;
                }
                Logger.LogDebug("Current event has finished playing out. Will play list from next event (Id: {0})", TransmissionList.Events[startIndex].Id);
            }
            Dictionary<IDevice, DeviceList> deviceLists = Scheduler.ScheduleTransmissionList(TransmissionList, DeviceListEventStore, startIndex);

            DeliverDeviceLists(deviceLists);
            if (deviceLists.Any())
            {
                TransmissionList.State = TransmissionListState.Playing;
            }
        }

        private int GetCurrentEventIndex()
        {
            return TransmissionList.CurrentEventId is null ? 0 : TransmissionList.GetEventPositionById(TransmissionList.CurrentEventId.Value);
        }

        public void NextTransmissionList()
        {            
            if(TransmissionList.Events.Count == 0)
            {
                return;
            }

            if(TransmissionList.State == TransmissionListState.Playing)
            {
                TransmissionListEvent currentEvent =  GetTransmissionListEventById(TransmissionList.CurrentEventId.Value);
                SetEventAsPlayed(currentEvent);
            }

            int currentPosition = GetCurrentEventIndex();
            int nextEventPosition = currentPosition + 1;

            if(nextEventPosition >= TransmissionList.Events.Count)
            {
                StopTransmissionList();
                return;
            }

            //TODO: Maybe we don't actually want this. Originally, I had this so that we could next multiple times easily
            //However, I think we need to take into account transitions here, so we should probably let the current event work as normal
            TransmissionList.CurrentEventId = TransmissionList.Events[nextEventPosition].Id;

            Dictionary<IDevice, DeviceList> deviceLists = Scheduler.ScheduleTransmissionList(TransmissionList, DeviceListEventStore, currentPosition);
            DeliverDeviceLists(deviceLists);
        }

        public void StopTransmissionList()
        {
            if (TransmissionList.State == TransmissionListState.Stopped)
            {
                return;
            }

            if (TransmissionList.CurrentEventId != null)
            {
                TransmissionListEvent effectedEvent = GetTransmissionListEventById(TransmissionList.CurrentEventId.Value);
                OnTransmissionListEventReset(effectedEvent);
            }

            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>();

            TransmissionList.Events.ForEach(transmissionEvent =>
            {
                transmissionEvent.EventFeatures.ForEach(feature =>
                {
                    if (feature.Device == null)
                    {
                        return;
                    }

                    feature.DeviceListEventId = null;
                    if (!deviceLists.ContainsKey(feature.Device))
                    {
                        deviceLists[feature.Device] = null;
                    }
                });
            });

            DeliverDeviceLists(deviceLists);
            TransmissionList.State = TransmissionListState.Stopped;
        }

        public void OnTransmissionListChanged(int changeIndex = 0)
        {
            //TODO: What happens if we move an event that has already played out?
            if(!TransmissionList.Events.Any())
            {
                TransmissionList.CurrentEventId = null;
                TransmissionList.State = TransmissionListState.Stopped;
                return;
            }

            if(TransmissionList.CurrentEventId is null)
            {
                TransmissionList.CurrentEventId = TransmissionList.Events[0].Id;
            }

            Logger.LogDebug("Transmission List has changed, current form: {0}", TransmissionList);

            changeIndex = GetCurrentEventIndex();
            //I wonder if we want to do some sort of dry run scheduling implementation?
            Dictionary<IDevice, DeviceList> deviceLists = Scheduler.ScheduleTransmissionList(TransmissionList, DeviceListEventStore, changeIndex);

            if(TransmissionList?.State == TransmissionListState.Playing)
            {
                DeliverDeviceLists(deviceLists);
            }

        }

        private void DeliverDeviceLists(Dictionary<IDevice, DeviceList> deviceLists)
        {
            deviceLists.ToList().ForEach((pair) => {
                Logger.LogDebug("Requesting device list change to '{0}', list: {1}", pair.Key?.Model?.Name, pair.Value);
                pair.Key.ActiveList = pair.Value;
            });
            UpdateDeviceSubscriptions();
        }

        private void UpdateDeviceSubscriptions()
        {
            if (TransmissionList is null)
            {
                return;
            }

            HashSet<IDevice> devices = TransmissionList.Events.SelectMany(listEvent => listEvent.EventFeatures.Select(feature => feature.Device)).ToHashSet();
            IEnumerable<IDevice> devicesToSubscribeTo = devices.Except(SubscribedDevices);
            foreach (IDevice device in devicesToSubscribeTo)
            {
                if(device is null)
                {
                    continue;
                }
                DeviceListEventWatcher.SubcsribeToDevice(this, device);
                SubscribedDevices.Add(device);
            }

            IEnumerable<IDevice> devicesToUnsubscribeFrom = SubscribedDevices.Except(devices);
            UnsubscribeFromDevices(devicesToUnsubscribeFrom);
        }

        private void UnsubscribeFromAllDeviceEventChanges(TransmissionList transmissionList)
        {
            if(transmissionList is null)
            {
                return;
            }

            HashSet<IDevice> devices = transmissionList.Events.SelectMany(listEvent => listEvent.EventFeatures.Select(feature => feature.Device)).ToHashSet();
            UnsubscribeFromDevices(devices);
        }

        private void UnsubscribeFromDevices(IEnumerable<IDevice> devices)
        {
            foreach (IDevice device in devices)
            {
                if (device is null)
                {
                    continue;
                }
                DeviceListEventWatcher.UnsubcsribeFromDevice(this, device);
                SubscribedDevices.Remove(device);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue && disposing)
            {
                UnsubscribeFromAllDeviceEventChanges(_transmissionList);
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
