using CBS.Siren.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using CBS.Siren.Utilities;

namespace CBS.Siren
{
    public class SimpleScheduler : IScheduler
    {
        public SimpleScheduler()
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
            return eventFeatures.Select(feature => feature.CalculateDuration()).Max();
        }

        private Dictionary<IDevice, DeviceList> GenerateDeviceLists(TransmissionList transmissionList)
        {
            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>();
            
            transmissionList.Events.ForEach((TransmissionListEvent transmissionEvent) => {
                TranslateListEventToDeviceEvents(transmissionEvent, deviceLists); //Not sure I'm happy with this implementation. It's relatively efficient, but relies a lot on state
            });

            return deviceLists;
        }

        private void TranslateListEventToDeviceEvents(TransmissionListEvent transmissionEvent, Dictionary<IDevice, DeviceList> deviceLists)
        {
            transmissionEvent.EventFeatures.ForEach(feature =>
            {
                if(feature.Device == null)
                {
                    return;
                }

                if (!deviceLists.ContainsKey(feature.Device))
                {
                    deviceLists[feature.Device] = new DeviceList(new List<DeviceListEvent>());
                }

                DeviceListEvent deviceListEvent = TranslateListEventFeature(transmissionEvent, feature);
                if (deviceListEvent != null)
                {
                    deviceLists[feature.Device].Events.Add(deviceListEvent);
                }
            });
        }

        private DeviceListEvent TranslateListEventFeature(TransmissionListEvent transmissionEvent, IEventFeature feature)
        {
            string eventData = GenerateEventData(transmissionEvent, feature);
            return new DeviceListEvent(eventData, transmissionEvent.Id);
        }

        private string GenerateEventData(TransmissionListEvent transmissionEvent, IEventFeature feature)
        {
            var timing = new { 
                StartTime = transmissionEvent.ExpectedStartTime,
                Duration = transmissionEvent.ExpectedDuration,
                EndTime = transmissionEvent.ExpectedStartTime.AddSeconds(transmissionEvent.ExpectedDuration.FramesToSeconds())
            };

            var source = new {
                StrategyData = feature?.SourceStrategy?.BuildStrategyData() ?? ""
            };

            var eventData = new
            {
                Timing = timing,
                Source = source
            };

            return eventData.SerializeObjectDataToJsonString();
        }
    }
}