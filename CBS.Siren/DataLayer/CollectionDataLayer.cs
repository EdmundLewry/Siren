using CBS.Siren.Device;
using CBS.Siren.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.Siren.Data
{
    public class CollectionDataLayer : IDataLayer
    {
        private int _nextListId = 0;
        private int _nextDeviceId = 0;
        private int _nextInstanceeId = 0;
        private Dictionary<int, TransmissionList> StoredTransmissionLists { get; set; } = new Dictionary<int, TransmissionList>();
        private Dictionary<int, MediaInstance> StoredMediaInstances { get; set; } = new Dictionary<int, MediaInstance>();
        private Dictionary<int, DeviceModel> StoredDevices { get; set; } = new Dictionary<int, DeviceModel>();

        public Task<IEnumerable<TransmissionList>> TransmissionLists()
        {
            return Task.FromResult<IEnumerable<TransmissionList>>(StoredTransmissionLists.Values);
        }
        
        public Task AddUpdateTransmissionLists(params TransmissionList[] lists)
        {
            foreach (var list in lists)
            {
                TransmissionList foundList = GetTransmissionListById(list.Id);
                if(foundList != null)
                {
                    /* For an in memory collection, we don't actually need to do anything
                     * if the user is updating the original object, since they would have updated the
                     * object reference.
                     
                    Note: Not sure exactly what I want this to behave like. Should Get pass back a new object?
                     */

                    foundList.SourceList = list.SourceList ?? foundList.SourceList;
                    foundList.Events = list.Events ?? foundList.Events;
                    AssignTransmissionListEventIds(foundList);
                    
                    continue;
                }

                list.Id = ++_nextListId;
                AssignTransmissionListEventIds(list);
                StoredTransmissionLists.Add(list.Id, list);
            }

            return Task.CompletedTask;
        }

        private void AssignTransmissionListEventIds(TransmissionList list)
        {
            List<TransmissionListEvent> eventsWithDefaultId = list.Events.Where(listEvent => listEvent.Id == default).ToList();
            eventsWithDefaultId.ForEach(listEvent => listEvent.Id = IdFactory.NextTransmissionListEventId());
        }

        private TransmissionList GetTransmissionListById(int id)
        {
            return StoredTransmissionLists.GetValueOrDefault(id);
        }

        public Task<List<MediaInstance>> AddUpdateMediaInstances(params MediaInstance[] instances)
        {
            List<MediaInstance> addedUpdatedInstances = new List<MediaInstance>();

            foreach (var mediaInstance in instances)
            {
                MediaInstance foundInstance = StoredMediaInstances.GetValueOrDefault(mediaInstance.Id);
                if (foundInstance != null)
                {
                    foundInstance.Name = string.IsNullOrWhiteSpace(mediaInstance.Name) ? foundInstance.Name : mediaInstance.Name;
                    foundInstance.Duration = mediaInstance.Duration == default ? foundInstance.Duration : mediaInstance.Duration;
                    foundInstance.FilePath = string.IsNullOrWhiteSpace(mediaInstance.FilePath) ? foundInstance.FilePath : mediaInstance.FilePath;
                    foundInstance.InstanceFileType = mediaInstance.InstanceFileType != foundInstance.InstanceFileType ? foundInstance.InstanceFileType : mediaInstance.InstanceFileType;
                    addedUpdatedInstances.Add(foundInstance);
                    continue;
                }

                mediaInstance.Id = ++_nextInstanceeId;
                StoredMediaInstances.Add(mediaInstance.Id, mediaInstance);
                addedUpdatedInstances.Add(mediaInstance);
            }

            return Task.FromResult(addedUpdatedInstances);
        }

        public Task<IEnumerable<MediaInstance>> MediaInstances()
        {
            return Task.FromResult<IEnumerable<MediaInstance>>(StoredMediaInstances.Values);
        }

        public Task<IEnumerable<DeviceModel>> Devices()
        {
            return Task.FromResult<IEnumerable<DeviceModel>>(StoredDevices.Values);
        }

        public Task<List<DeviceModel>> AddUpdateDevices(params DeviceModel[] devices)
        {
            List<DeviceModel> addedUpdatedDevices = new List<DeviceModel>();

            foreach (var device in devices)
            {
                DeviceModel foundDevice = StoredDevices.GetValueOrDefault(device.Id);
                if (foundDevice != null)
                {
                    foundDevice.Name = string.IsNullOrWhiteSpace(device.Name) ? foundDevice.Name : device.Name;
                    addedUpdatedDevices.Add(foundDevice);
                    continue;
                }

                device.Id = ++_nextDeviceId;
                StoredDevices.Add(device.Id, device);
                addedUpdatedDevices.Add(device);
            }

            return Task.FromResult(addedUpdatedDevices);
        }
    }
}