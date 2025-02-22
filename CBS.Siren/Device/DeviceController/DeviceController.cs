﻿using System;
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

        public ITimeSourceProvider Clock { get; set; } = TimeSource.TimeProvider;

        public DeviceController(ILogger logger, IDeviceListEventStore deviceListEventStore)
        {
            _logger = logger;
            DeviceListEventStore = deviceListEventStore;
        }

        private void UpdateActiveDeviceList(DeviceList value)
        {
            lock (_deviceListLock)
            {
                if (value is null || value.Events.Count == 0)
                {
                    Reset();
                    _logger.LogInformation($"Device List has been reset");
                    return;
                }

                DeviceList deviceList = MatchDeviceListEventStates(value);
                Reset();

                _activeDeviceList = deviceList;
                _activeDeviceList.Events.ForEach(listEvent => {
                    if(listEvent.EventState.CurrentStatus == DeviceListEventStatus.UNSCHEDULED)
                    {
                        listEvent.EventState.CurrentStatus = DeviceListEventStatus.CUED;
                    }
                });
                EventIndex = _activeDeviceList.Events.Count > 0 ? 0 : INVALID_INDEX;
                if(CurrentEvent.EventState.CurrentStatus == DeviceListEventStatus.PLAYING)
                {
                    _eventHasStarted = true;
                }
            }
            _logger.LogInformation($"Device List with {_activeDeviceList.Events.Count} events has been set");
            _logger.LogDebug("Device List has been set to {0}", _activeDeviceList);
        }

        private void Reset()
        {
            EventIndex = INVALID_INDEX;
            _eventHasStarted = false;
            _activeDeviceList = null;
        }

        private DeviceList MatchDeviceListEventStates(DeviceList value)
        {
            DeviceList incomingList = new DeviceList(value);
            _activeDeviceList?.Events.ForEach(listEvent =>
            {
                int index = incomingList.Events.FindIndex(targetEvent => targetEvent.Id == listEvent.Id);
                if(index >= 0)
                {
                    incomingList.Events[index].EventState = listEvent.EventState;
                }
            });

            return incomingList;
        }

        public async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                lock(_deviceListLock)
                {
                    bool listPlaying = false;

                    try
                    {
                        listPlaying = ListIsPlaying();
                    }
                    catch(Exception e)
                    {
                        _logger.LogError("Exception while checking for list playing! EventIndex - {0} - {1}", EventIndex, e);
                        throw;
                    }
                    if(listPlaying)
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

        private bool TimeHasPassed(DateTimeOffset timeToCheck)
        {
            if (timeToCheck.DifferenceInFrames(Clock.Now) >= 0)
            {
                return true;
            }

            return false;
        }

        private bool CheckForEventEnd()
        {
            return TimeHasPassed(CurrentEvent.EndTime);
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
            endedEvent.EventState.CurrentStatus = DeviceListEventStatus.PLAYED;
            DeviceListEventStore.UpdateDeviceListEvent(endedEvent);
            OnEventEnded?.Invoke(this, new DeviceEventChangedEventArgs(endedEvent));
        }

        private bool CheckForEventStart()
        {
            return TimeHasPassed(CurrentEvent.StartTime);
        }

        private void CurrentEventStarted()
        {
            _eventHasStarted = true;
            CurrentEvent.EventState.CurrentStatus = DeviceListEventStatus.PLAYING;
            DeviceListEventStore.UpdateDeviceListEvent(CurrentEvent);
            OnEventStarted?.Invoke(this, new DeviceEventChangedEventArgs(CurrentEvent));
        }
    }
}
