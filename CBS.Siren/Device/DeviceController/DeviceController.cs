using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CBS.Siren.Time;
using Microsoft.Extensions.Logging;

namespace CBS.Siren.Device
{
    public class DeviceController : IDeviceController
    {
        private readonly ILogger _logger;
        private const int INVALID_INDEX = -1;

        private bool _eventHasStarted = false;
        private readonly object _deviceListLock = new object();

        public event EventHandler<DeviceEventChangedEventArgs> OnEventStarted = delegate { };
        public event EventHandler<DeviceEventChangedEventArgs> OnEventEnded = delegate { };
        public event EventHandler<EventArgs> OnDeviceListEnded = delegate { };

        private DeviceList _activeDeviceList;
        public DeviceList ActiveDeviceList { 
            get => _activeDeviceList; 
            set { UpdateActiveDeviceList(value); } 
        }

        private int EventIndex { get; set; } = INVALID_INDEX;
        public DeviceListEvent CurrentEvent { get => EventIndex==INVALID_INDEX ? null : ActiveDeviceList.Events[EventIndex]; }
        public IDeviceListEventStore DeviceListEventStore { get; }

        public DeviceController(ILogger logger, IDeviceListEventStore deviceListEventStore)
        {
            _logger = logger;
            DeviceListEventStore = deviceListEventStore;
        }

        private void UpdateActiveDeviceList(DeviceList value)
        {
            lock (_deviceListLock)
            {
                if (_activeDeviceList == null)
                {
                    _activeDeviceList = new DeviceList(value);
                    _activeDeviceList.Events.ForEach(listEvent => listEvent.EventState.CurrentStatus = DeviceListEventState.Status.CUED);
                    EventIndex = _activeDeviceList.Events.Count > 0 ? 0 : INVALID_INDEX;
                }
                else
                {
                    DeviceList incomingList = new DeviceList(value);
                    int replacePosition = FindReplacePosition(incomingList);
                    PrepareListsForReplace(incomingList, replacePosition);

                    incomingList.Events.ForEach(listEvent => listEvent.EventState.CurrentStatus = DeviceListEventState.Status.CUED);
                    _activeDeviceList.Events.AddRange(incomingList.Events);
                }

            }
            _logger.LogInformation($"Device List with {_activeDeviceList.Events.Count} events has been set");
        }

        private void PrepareListsForReplace(DeviceList value, int replacePosition)
        {
            if (replacePosition == -1)
            {
                value.Events.RemoveRange(0, _activeDeviceList.Events.Count);
                return;
            }

            _activeDeviceList.Events.RemoveRange(replacePosition, _activeDeviceList.Events.Count - replacePosition);
            if (replacePosition <= value.Events.Count)
            {
                value.Events.RemoveRange(0, replacePosition);
            }
        }

        private int FindReplacePosition(DeviceList incomingList)
        {
            for (int i = 0; i < _activeDeviceList.Events.Count; ++i)
            {
                if (i >= incomingList.Events.Count || _activeDeviceList.Events[i].Id != incomingList.Events[i].Id)
                {
                    return i;
                }
            }

            return -1;
        }

        public async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                lock(_deviceListLock)
                {
                    if(ListIsPlaying())
                    {
                        if(CheckForEventEnd())
                        {
                            CurrentEventEnded();
                        }

                        if(CurrentEvent != null && !_eventHasStarted && CheckForEventStart())
                        {
                            CurrentEventStarted();
                        }
                    }
                }
                
                await Task.Delay(5); //Stops us thrashing the CPU
            }
        }

        private bool ListIsPlaying() => ActiveDeviceList?.Events.Count > 0 && CurrentEvent != null;

        private bool TimeHasPassed(string timeToCheck)
        {
            DateTime expectedTime = DateTimeExtensions.FromTimecodeString(timeToCheck);

            if (expectedTime.DifferenceInFrames(DateTime.Now) >= 0)
            {
                return true;
            }

            return false;
        }

        private bool CheckForEventEnd()
        {
            JsonElement endTimeElement = JsonDocument.Parse(CurrentEvent.EventData).RootElement.GetProperty("timing").GetProperty("endTime");
            return TimeHasPassed(endTimeElement.GetString());
        }

        private void CurrentEventEnded()
        {
            DeviceListEvent endedEvent = CurrentEvent;
            if(_activeDeviceList.Events.Count > EventIndex + 1)
            {
                EventIndex++;
            }
            else
            {
                EventIndex = INVALID_INDEX;
                OnDeviceListEnded?.Invoke(this, EventArgs.Empty);
                _activeDeviceList = null;
            }

            _eventHasStarted = false;
            endedEvent.EventState.CurrentStatus = DeviceListEventState.Status.PLAYED;
            DeviceListEventStore.UpdateDeviceListEvent(endedEvent);
            OnEventEnded?.Invoke(this, new DeviceEventChangedEventArgs(endedEvent));
        }

        private bool CheckForEventStart()
        {
            JsonElement startTimeElement = JsonDocument.Parse(CurrentEvent.EventData).RootElement.GetProperty("timing").GetProperty("startTime");
            return TimeHasPassed(startTimeElement.GetString());
        }

        private void CurrentEventStarted()
        {
            _eventHasStarted = true;
            CurrentEvent.EventState.CurrentStatus = DeviceListEventState.Status.PLAYING;
            DeviceListEventStore.UpdateDeviceListEvent(CurrentEvent);
            OnEventStarted?.Invoke(this, new DeviceEventChangedEventArgs(CurrentEvent));
        }
    }
}
