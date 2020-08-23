using System.Collections.Generic;
using System.Linq;
using CBS.Siren.Utilities;
using CBS.Siren.Device;
using System;
using CBS.Siren.Time;

namespace CBS.Siren
{
    public class SimpleScheduler : IScheduler
    {
        public Dictionary<IDevice, DeviceList> ScheduleTransmissionList(TransmissionList transmissionList, IDeviceListEventFactory deviceListEventFactory)
        {
            TransmissionList calculatedTransmsmissionList = CalculateListTimings(transmissionList);
            return GenerateDeviceLists(calculatedTransmsmissionList, deviceListEventFactory);
        }

        private TransmissionList CalculateListTimings(TransmissionList transmissionList)
        {
            transmissionList.Events.ForEach((TransmissionListEvent transmissionEvent) => {
                transmissionEvent.ExpectedStartTime = transmissionEvent.EventTimingStrategy.CalculateStartTime(transmissionEvent.Id, transmissionList);
                transmissionEvent.ExpectedDuration = CalculateLongestFeatureDuration(transmissionEvent.EventFeatures);
            });

            return transmissionList;
        }

        private TimeSpan CalculateLongestFeatureDuration(List<IEventFeature> eventFeatures)
        {
            return eventFeatures.Select(feature => feature.CalculateDuration()).Max();
        }

        private Dictionary<IDevice, DeviceList> GenerateDeviceLists(TransmissionList transmissionList, IDeviceListEventFactory deviceListEventFactory)
        {
            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>();
            
            transmissionList.Events.ForEach((transmissionEvent) => {
                TranslateListEventToDeviceEvents(transmissionEvent, deviceLists, deviceListEventFactory); //Not sure I'm happy with this implementation. It's relatively efficient, but relies a lot on state
                transmissionEvent.EventState.CurrentStatus = TransmissionListEventState.Status.SCHEDULED;
            });

            return deviceLists;
        }

        private void TranslateListEventToDeviceEvents(TransmissionListEvent transmissionEvent, Dictionary<IDevice, DeviceList> deviceLists, IDeviceListEventFactory deviceListEventFactory)
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

                DeviceListEvent deviceListEvent = TranslateListEventFeature(transmissionEvent, feature, deviceListEventFactory);
                if (deviceListEvent != null)
                {
                    deviceLists[feature.Device].Events.Add(deviceListEvent);
                }
            });
        }

        private DeviceListEvent TranslateListEventFeature(TransmissionListEvent transmissionEvent, IEventFeature feature, IDeviceListEventFactory deviceListEventFactory)
        {
            string eventData = GenerateEventData(transmissionEvent, feature);
            DeviceListEvent createdEvent = deviceListEventFactory.CreateDeviceListEvent(eventData, transmissionEvent.Id);
            feature.DeviceListEventId = createdEvent.Id;
            return createdEvent;
        }

        private string GenerateEventData(TransmissionListEvent transmissionEvent, IEventFeature feature)
        {
            var timing = new { 
                StartTime = transmissionEvent.ExpectedStartTime.ToTimecodeString(),
                Duration = transmissionEvent.ExpectedDuration.ToTimecodeString(),
                EndTime = transmissionEvent.ExpectedStartTime.AddSeconds(transmissionEvent.ExpectedDuration.TotalSeconds).ToTimecodeString()
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