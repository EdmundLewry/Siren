using CBS.Siren.Device;
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
        private List<TransmissionList> StoredTransmissionLists { get; set; } = new List<TransmissionList>();
        private List<MediaInstance> StoredMediaInstances { get; set; } = new List<MediaInstance>();
        private List<DeviceModel> StoredDevices { get; set; } = new List<DeviceModel>();

        public Task<IEnumerable<TransmissionList>> TransmissionLists()
        {
            return Task.FromResult<IEnumerable<TransmissionList>>(StoredTransmissionLists);
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
                    continue;
                }

                list.Id = _nextListId++;
                StoredTransmissionLists.Add(list);
            }

            return Task.CompletedTask;
        }

        private TransmissionList GetTransmissionListById(int? id)
        {
            return StoredTransmissionLists.FirstOrDefault((list) => list.Id == id);
        }

        public Task<List<MediaInstance>> AddUpdateMediaInstances(params MediaInstance[] instances)
        {
            List<MediaInstance> addedUpdatedInstances = new List<MediaInstance>();

            foreach (var mediaInstance in instances)
            {
                MediaInstance foundInstance = mediaInstance.Id == null ? null : StoredMediaInstances.FirstOrDefault((storedInstance) => storedInstance.Id == mediaInstance.Id);
                if (foundInstance != null)
                {
                    foundInstance.Name = string.IsNullOrWhiteSpace(mediaInstance.Name) ? foundInstance.Name : mediaInstance.Name;
                    foundInstance.Duration = mediaInstance.Duration == default ? foundInstance.Duration : mediaInstance.Duration;
                    foundInstance.FilePath = string.IsNullOrWhiteSpace(mediaInstance.FilePath) ? foundInstance.FilePath : mediaInstance.FilePath;
                    foundInstance.InstanceFileType = mediaInstance.InstanceFileType != foundInstance.InstanceFileType ? foundInstance.InstanceFileType : mediaInstance.InstanceFileType;
                    addedUpdatedInstances.Add(foundInstance);
                    continue;
                }

                mediaInstance.Id = _nextInstanceeId++;
                StoredMediaInstances.Add(mediaInstance);
                addedUpdatedInstances.Add(mediaInstance);
            }

            return Task.FromResult(addedUpdatedInstances);
        }

        public Task<IEnumerable<MediaInstance>> MediaInstances()
        {
            return Task.FromResult<IEnumerable<MediaInstance>>(StoredMediaInstances);
        }

        public Task<IEnumerable<DeviceModel>> Devices()
        {
            return Task.FromResult<IEnumerable<DeviceModel>>(StoredDevices);
        }

        public Task<List<DeviceModel>> AddUpdateDevices(params DeviceModel[] devices)
        {
            List<DeviceModel> addedUpdatedDevices = new List<DeviceModel>();

            foreach (var device in devices)
            {
                DeviceModel foundDevice = device.Id == null ? null : StoredDevices.FirstOrDefault((storedDevice) => storedDevice.Id == device.Id);
                if (foundDevice != null)
                {
                    foundDevice.Name = string.IsNullOrWhiteSpace(device.Name) ? foundDevice.Name : device.Name;
                    addedUpdatedDevices.Add(foundDevice);
                    continue;
                }

                device.Id = _nextDeviceId++;
                StoredDevices.Add(device);
                addedUpdatedDevices.Add(device);
            }

            return Task.FromResult(addedUpdatedDevices);
        }
    }
}