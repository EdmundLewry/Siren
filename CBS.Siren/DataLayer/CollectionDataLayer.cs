using CBS.Siren.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.Siren.Data
{
    public class CollectionDataLayer : IDataLayer
    {
        private long nextListId = 0;
        private int nextDeviceId = 0;
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

                list.Id = nextListId++.ToString();
                StoredTransmissionLists.Add(list);
            }

            return Task.CompletedTask;
        }

        private TransmissionList GetTransmissionListById(string id)
        {
            return StoredTransmissionLists.FirstOrDefault((list) => list.Id == id);
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

                device.Id = nextDeviceId++;
                StoredDevices.Add(device);
                addedUpdatedDevices.Add(device);
            }

            return Task.FromResult(addedUpdatedDevices);
        }
    }
}