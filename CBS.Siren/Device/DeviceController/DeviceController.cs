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

        private DeviceList _activeDeviceList;
        public DeviceList ActiveDeviceList { 
            get => _activeDeviceList; 
            set {
                lock (_deviceListLock)
                {
                    _activeDeviceList = value;
                    EventIndex = _activeDeviceList.Events.Count > 0 ? 0 : INVALID_INDEX;
                }
                _logger.LogInformation($"Device List with {_activeDeviceList.Events.Count} events has been set");
            } 
        }

        private int EventIndex { get; set; } = INVALID_INDEX;
        public DeviceListEvent CurrentEvent { get => EventIndex==INVALID_INDEX ? null : ActiveDeviceList.Events[EventIndex]; }

        public DeviceController(ILogger logger)
        {
            _logger = logger;
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
                
                await Task.Delay(5);
            }
        }

        private bool ListIsPlaying() => ActiveDeviceList?.Events.Count > 0 && CurrentEvent != null;

        private bool TimeHasPassed(string timeToCheck)
        {
            DateTime expectedTime = DateTime.Parse(timeToCheck);

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
            }

            _eventHasStarted = false;
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
            OnEventStarted?.Invoke(this, new DeviceEventChangedEventArgs(CurrentEvent));
        }
    }
}
