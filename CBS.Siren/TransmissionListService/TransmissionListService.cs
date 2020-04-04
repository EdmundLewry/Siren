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

        private TransmissionList _transmissionList;
        //TODO: If the list is playing, we shouldn't be able to replace it
        public TransmissionList TransmissionList { get => _transmissionList; 
                                                   set => _transmissionList = value; }
        public TransmissionListService(IScheduler scheduler, ILogger<TransmissionListService> logger)
        {
            Logger = logger;
            Scheduler = scheduler;
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
    }
}
