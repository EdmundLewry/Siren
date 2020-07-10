using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CBS.Siren.DTO;
using CBS.Siren.Data;
using CBS.Siren.Device;
using Microsoft.Extensions.Logging;
using System;

namespace CBS.Siren.Application
{
    public class TransmissionListHandler : ITransmissionListHandler
    {
        public ILogger<TransmissionListHandler> Logger { get; }
        public IDataLayer DataLayer { get; }
        public ITransmissionListService TransmissionListService { get; }
        public Channel Channel { get; }

        public TransmissionListHandler(ILogger<TransmissionListHandler> logger, IDataLayer dataLayer, ITransmissionListService transmissionListService, IDeviceManager deviceManager)
        {
            Logger = logger;
            DataLayer = dataLayer;
            TransmissionListService = transmissionListService;

            Channel = GenerateChannel(deviceManager);
        }

        private Channel GenerateChannel(IDeviceManager deviceManager)
        {
            List<DeviceModel> deviceModels = DataLayer.Devices().Result.ToList();
            List<IDevice> devices = deviceModels.Select(model => deviceManager.GetDevice(model.Id.GetValueOrDefault())).ToList();
            VideoChain chainConfiguration = new VideoChain(devices);

            return new Channel(chainConfiguration);
        }

        private async Task<TransmissionList> GetListBydId(string id)
        {
            IEnumerable<TransmissionList> transmissionLists = await DataLayer.TransmissionLists();
            TransmissionList transmissionList = transmissionLists.FirstOrDefault(list => list.Id == id);

            if (transmissionList == null)
            {
                throw new ArgumentException($"Unable to find list with id: {id}", "id");
            }

            return transmissionList;
        }

        public async Task<IEnumerable<TransmissionList>> GetAllLists()
        {
            return await DataLayer.TransmissionLists();
        }

        public async Task<IEnumerable<TransmissionListEvent>> GetListEvents(string id)
        {
            TransmissionList transmissionList = await GetListBydId(id);
            return transmissionList.Events;
        }

        public async Task<TransmissionListEvent> AddEvent(string id, TransmissionListEventCreationDTO listEvent)
        {
            TransmissionList transmissionList = await GetListBydId(id);

            TransmissionListEvent createdEvent = TransmissionListEventFactory.BuildTransmissionListEvent(listEvent.TimingData, listEvent.Features, Channel.ChainConfiguration, DataLayer);
            transmissionList.Events.Add(createdEvent);
            await DataLayer.AddUpdateTransmissionLists(transmissionList);
            return createdEvent;
        }

        public async Task RemoveEvent(string listId, string eventId)
        {
            TransmissionList transmissionList = await GetListBydId(listId);

            TransmissionListEvent listEvent = transmissionList.Events.FirstOrDefault(listEvent => listEvent.Id.ToString() == eventId);
            if(listEvent == null)
            {
                throw new ArgumentException($"Unable to find list event with id: {eventId}", "eventId");
            }

            transmissionList.Events.Remove(listEvent);
            await DataLayer.AddUpdateTransmissionLists(transmissionList);
        }

        public async Task ClearList(string id)
        {
            TransmissionList transmissionList = await GetListBydId(id);
            transmissionList.Events.Clear();

            await DataLayer.AddUpdateTransmissionLists(transmissionList);
        }

        public async Task PlayTransmissionList(string id)
        {
            TransmissionList transmissionList = await GetListBydId(id);
            TransmissionListService.TransmissionList = transmissionList;

            TransmissionListService.PlayTransmissionList();
            await DataLayer.AddUpdateTransmissionLists(transmissionList);
        }

        public Task PauseTransmissionList(string id)
        {
            throw new NotImplementedException();
        }

        public Task NextTransmissionList(string id)
        {
            throw new NotImplementedException();
        }
    }
}