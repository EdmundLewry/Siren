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
        public ITransmissionListServiceStore TransmissionListServiceStore { get; }
        public Channel Channel { get; }

        public TransmissionListHandler(ILogger<TransmissionListHandler> logger, IDataLayer dataLayer, ITransmissionListServiceStore transmissionListServiceStore, IDeviceManager deviceManager)
        {
            Logger = logger;
            DataLayer = dataLayer;
            TransmissionListServiceStore = transmissionListServiceStore;

            Channel = GenerateChannel(deviceManager);
        }

        private Channel GenerateChannel(IDeviceManager deviceManager)
        {
            List<DeviceModel> deviceModels = DataLayer.Devices().Result.ToList();
            List<IDevice> devices = deviceModels.Select(model => deviceManager.GetDevice(model.Id)).ToList();
            VideoChain chainConfiguration = new VideoChain(devices);

            return new Channel(chainConfiguration);
        }

        private async Task<TransmissionList> GetListBydId(int id)
        {
            IEnumerable<TransmissionList> transmissionLists = await DataLayer.TransmissionLists();

            TransmissionList transmissionList = transmissionLists.FirstOrDefault(list => list.Id == id);

            if (transmissionList == null)
            {
                Logger.LogError($"Unable to find list with Id {id}");
                throw new ArgumentException($"Unable to find list with id: {id}", "id");
            }

            return transmissionList;
        }

        public async Task<IEnumerable<TransmissionList>> GetAllLists()
        {
            return await DataLayer.TransmissionLists();
        }

        public async Task<IEnumerable<TransmissionListEvent>> GetListEvents(int id)
        {
            TransmissionList transmissionList = await GetListBydId(id);
            return transmissionList.Events;
        }

        public async Task<TransmissionListEvent> AddEvent(int id, TransmissionListEventCreationDTO listEvent)
        {
            TransmissionList transmissionList = await GetListBydId(id);
            if (transmissionList == null)
            {
                Logger.LogError($"Unable to find list with Id {id}");
                throw new ArgumentException($"Unable to find list with id: {id}", "id");
            }

            TransmissionListEvent createdEvent = TransmissionListEventFactory.BuildTransmissionListEvent(listEvent.TimingData, listEvent.Features, Channel.ChainConfiguration, DataLayer);
            int insertedAtPosition = InsertEventIntoList(createdEvent, listEvent.ListPosition, transmissionList);
            await DataLayer.AddUpdateTransmissionLists(transmissionList);

            ITransmissionListService transmissionListService = TransmissionListServiceStore.GetTransmissionListServiceByListId(id);
            transmissionListService?.OnTransmissionListChanged(insertedAtPosition);
            return createdEvent;
        }

        private int InsertEventIntoList(TransmissionListEvent createdEvent, ListPositionDTO listPosition, TransmissionList transmissionList)
        {
            if(listPosition != null)
            {
                int foundEventIndex = GetEventPositionById(transmissionList, listPosition.AssociatedEventId);
                if(foundEventIndex != -1)
                {
                    foundEventIndex = listPosition.RelativePosition == RelativePosition.Above ? foundEventIndex : foundEventIndex + 1;
                    transmissionList.Events.Insert(foundEventIndex, createdEvent);
                    return foundEventIndex;
                }
            }

            transmissionList.Events.Add(createdEvent);
            return transmissionList.Events.Count-1;
        }

        private int GetEventPositionById(TransmissionList transmissionList, int eventId)
        {
            //This is a fairly expensive way to do this. We may want to add positional data 
            //to the events and maintain it in some other way
            //We'll probably need that so that we can order them when they come out of the db anyway
            return transmissionList.Events.FindIndex((listEvent) => listEvent.Id == eventId);
        }

        public async Task RemoveEvent(int listId, int eventId)
        {
            TransmissionList transmissionList = await GetListBydId(listId);
            if (transmissionList == null)
            {
                Logger.LogError($"Unable to find list with Id {listId}");
                throw new ArgumentException($"Unable to find list with id: {listId}", "listId");
            }

            int listEventPositionIndex = GetEventPositionById(transmissionList, eventId);
            if(listEventPositionIndex == -1)
            {
                throw new ArgumentException($"Unable to find list event with id: {eventId}", "eventId");
            }

            TransmissionListEvent listEvent = transmissionList.Events[listEventPositionIndex];

            transmissionList.Events.Remove(listEvent);
            await DataLayer.AddUpdateTransmissionLists(transmissionList);

            ITransmissionListService transmissionListService = TransmissionListServiceStore.GetTransmissionListServiceByListId(listId);
            transmissionListService?.OnTransmissionListChanged(listEventPositionIndex);
        }

        public async Task ClearList(int id)
        {
            TransmissionList transmissionList = await GetListBydId(id);
            transmissionList.Events.Clear();

            await DataLayer.AddUpdateTransmissionLists(transmissionList);
        }

        public async Task PlayTransmissionList(int id)
        {
            TransmissionList transmissionList = await GetListBydId(id);
            if (transmissionList == null)
            {
                string message = $"Unable to find list with Id {id}";
                Logger.LogError(message);
                throw new ArgumentException(message, nameof(id));
            }

            ITransmissionListService transmissionListService = TransmissionListServiceStore.GetTransmissionListServiceByListId(id);
            if (transmissionListService == null)
            {
                string message = $"Unable to find list service for list with Id {id}";
                Logger.LogError(message);
                throw new ArgumentException(message, nameof(id));
            }

            transmissionListService.PlayTransmissionList();
            await DataLayer.AddUpdateTransmissionLists(transmissionList);
        }

        public Task PauseTransmissionList(int id)
        {
            throw new NotImplementedException();
        }

        public Task NextTransmissionList(int id)
        {
            throw new NotImplementedException();
        }
    }
}