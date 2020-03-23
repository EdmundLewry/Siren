using System;
using System.Threading;
using System.Threading.Tasks;

namespace CBS.Siren.Device
{
    public class DeviceController : IDeviceController
    {
        private const int INVALID_INDEX = -1;

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
            } 
        } 
        
        private int EventIndex { get; set; }
        public DeviceListEvent CurrentEvent { get => EventIndex==INVALID_INDEX ? null : ActiveDeviceList.Events[EventIndex]; }

        public DeviceController()
        {

        }

        public async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                lock(_deviceListLock)
                {
                    if(ActiveDeviceList?.Events.Count > 0)
                    {
                        if(CheckForEventEnd())
                        {
                            CurrentEventEnded();
                        }

                        if(CheckForEventStart())
                        {
                            CurrentEventStarted();
                        }
                    }
                }
                
                await Task.Delay(10);
            }
        }

        private bool CheckForEventEnd()
        {
            return true;
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

            OnEventEnded?.Invoke(this, new DeviceEventChangedEventArgs(endedEvent));
        }

        private bool CheckForEventStart()
        {
            return false;
        }

        private void CurrentEventStarted()
        {
            
        }
    }
}
