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
        public Dictionary<IDevice, DeviceList> ScheduleTransmissionList(TransmissionList transmissionList, IDeviceListEventStore deviceListEventStore, int startIndex = 0)
        {
            TransmissionList calculatedTransmsmissionList = CalculateListTimings(transmissionList, startIndex);
            return GenerateDeviceLists(calculatedTransmsmissionList, deviceListEventStore, startIndex);
        }

        private TransmissionList CalculateListTimings(TransmissionList transmissionList, int startIndex)
        {
            for(int i = startIndex; i<transmissionList.Events.Count; ++i)
            {
                TransmissionListEvent transmissionEvent = transmissionList.Events[i];
                transmissionEvent.ExpectedStartTime = transmissionEvent.EventTimingStrategy.CalculateStartTime(transmissionEvent.Id, transmissionList);
                transmissionEvent.ExpectedDuration = CalculateLongestFeatureDuration(transmissionEvent.EventFeatures);
            }

            return transmissionList;
        }

        private TimeSpan CalculateLongestFeatureDuration(List<IEventFeature> eventFeatures)
        {
            return eventFeatures.Select(feature => feature.CalculateDuration()).Max();
        }

        private Dictionary<IDevice, DeviceList> GenerateDeviceLists(TransmissionList transmissionList, IDeviceListEventStore deviceListEventStore, int startIndex)
        {
            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>();

            for (int i = startIndex; i < transmissionList.Events.Count; ++i)
            {
                TransmissionListEvent transmissionEvent = transmissionList.Events[i];
                TranslateListEventToDeviceEvents(transmissionEvent, deviceLists, deviceListEventStore); //Not sure I'm happy with this implementation. It's relatively efficient, but relies a lot on state
                if(transmissionEvent.EventState.CurrentStatus == TransmissionListEventState.Status.UNSCHEDULED)
                {
                    transmissionEvent.EventState.CurrentStatus = TransmissionListEventState.Status.SCHEDULED;
                }
            }

            return deviceLists;
        }

        private void TranslateListEventToDeviceEvents(TransmissionListEvent transmissionEvent, Dictionary<IDevice, DeviceList> deviceLists, IDeviceListEventStore deviceListEventStore)
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

                DeviceListEvent deviceListEvent = TranslateListEventFeature(transmissionEvent, feature, deviceListEventStore);
                if (deviceListEvent != null)
                {
                    deviceLists[feature.Device].Events.Add(deviceListEvent);
                }
            });
        }

        private DeviceListEvent TranslateListEventFeature(TransmissionListEvent transmissionEvent, IEventFeature feature, IDeviceListEventStore deviceListEventStore)
        {
            string eventData = GenerateEventData(transmissionEvent, feature);
            DeviceListEvent deviceEvent;
            if (feature.DeviceListEventId.HasValue)
            {
                deviceEvent =  deviceListEventStore.GetEventById(feature.DeviceListEventId.Value);
                deviceEvent.EventData = eventData;
                deviceEvent.RelatedTransmissionListEventId = transmissionEvent.Id;
            }
            else
            {
                deviceEvent = deviceListEventStore.CreateDeviceListEvent(eventData, transmissionEvent.Id);
            }

            feature.DeviceListEventId = deviceEvent.Id;
            return deviceEvent;
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