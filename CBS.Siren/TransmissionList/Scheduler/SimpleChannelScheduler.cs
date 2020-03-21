using System;
using System.Collections.Generic;

namespace CBS.Siren
{
    public class SimpleChannelScheduler : IScheduler
    {
        public SimpleChannelScheduler()
        {
            
        }

        public Dictionary<IDevice, DeviceList> ScheduleTransmissionList(TransmissionList transmissionList)
        {
            TransmissionList calculatedTransmsmissionList = CalculateListTimings(transmissionList);
            return GenerateDeviceLists(calculatedTransmsmissionList);
        }

        private TransmissionList CalculateListTimings(TransmissionList transmissionList)
        {
            transmissionList.Events.ForEach((TransmissionListEvent transmissionEvent) => {
                transmissionEvent.ExpectedStartTime = transmissionEvent.EventTimingStrategy.CalculateStartTime();
                transmissionEvent.ExpectedDuration = CalculateLongestFeatureDuration(transmissionEvent.EventFeatures);
            });

            return transmissionList;
        }

        private int CalculateLongestFeatureDuration(List<IEventFeature> eventFeatures)
        {
            return 0;
        }

        private Dictionary<IDevice, DeviceList> GenerateDeviceLists(TransmissionList transmissionList)
        {
            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>();
            
            transmissionList.Events.ForEach((TransmissionListEvent transmissionEvent) => {

                DeviceListEvent deviceListEvent = TranslateListEvent(transmissionEvent);
                if (deviceListEvent != null)
                {
                    if (!deviceLists.ContainsKey(transmissionEvent.Device))
                    {
                        deviceLists[transmissionEvent.Device] = new DeviceList(new List<DeviceListEvent>());
                    }
                    deviceLists[transmissionEvent.Device].Events.Add(deviceListEvent);
                }
            });

            return deviceLists;
        }

        private DeviceListEvent TranslateListEvent(TransmissionListEvent transmissionEvent)
        {

            string eventData = GenerateEventDate(transmissionEvent);
            return new DeviceListEvent(eventData, transmissionEvent.Id);
        }

        private string GenerateEventDate(TransmissionListEvent transmissionEvent)
        {
            return "";
        }

    }
}