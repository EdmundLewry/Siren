using CBS.Siren.Device;
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
        //TODO: If the list is playing, we shouldn't be able to replace it
        public TransmissionList TransmissionList 
        { 
            get => _transmissionList; 
            set {
                UnsubscriveFromDeviceEventChanges();
                _transmissionList = value;
                SubscribeToDeviceEvents();
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

        private void SubscribeToDeviceEvents()
        {
            throw new NotImplementedException();
        }

        private void UnsubscriveFromDeviceEventChanges()
        {
            throw new NotImplementedException();
        }
    }
}
