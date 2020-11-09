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

        public async Task<TransmissionList> GetListById(int id)
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
            TransmissionList transmissionList = await GetListById(id);
            return transmissionList.Events;
        }

        public async Task<TransmissionListEvent> GetListEventById(int listId, int eventId)
        {
            TransmissionList transmissionList = await GetListById(listId);

            TransmissionListEvent foundEvent = transmissionList.Events.FirstOrDefault(listEvent => listEvent.Id == eventId);
            if(foundEvent is null)
            {
                Logger.LogError("Unable to find list event with id: {TransmissionListEventId}", eventId);
                throw new ArgumentException($"Unable to find list event with id: {eventId}", nameof(eventId));
            }
            return foundEvent;
        }

        public async Task<TransmissionListEvent> AddEvent(int id, TransmissionListEventUpsertDTO listEvent)
        {
            TransmissionList transmissionList = await GetListById(id);

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
                int foundEventIndex = transmissionList.GetEventPositionById(listPosition.AssociatedEventId);
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

        public async Task RemoveEvent(int listId, int eventId)
        {
            TransmissionList transmissionList = await GetListById(listId);
            int listEventPositionIndex = transmissionList.GetEventPositionById(eventId);

            TransmissionListEvent listEvent = transmissionList.Events[listEventPositionIndex];

            transmissionList.Events.Remove(listEvent);
            await DataLayer.AddUpdateTransmissionLists(transmissionList);

            ITransmissionListService transmissionListService = TransmissionListServiceStore.GetTransmissionListServiceByListId(listId);
            transmissionListService?.OnTransmissionListChanged(listEventPositionIndex);
        }

        public async Task ClearList(int id)
        {
            TransmissionList transmissionList = await GetListById(id);
            transmissionList.Events.Clear();
            transmissionList.CurrentEventId = null;

            ITransmissionListService transmissionListService = TransmissionListServiceStore.GetTransmissionListServiceByListId(id);
            transmissionListService?.OnTransmissionListChanged();

            await DataLayer.AddUpdateTransmissionLists(transmissionList);
        }

        public async Task PlayTransmissionList(int id)
        {
            TransmissionList transmissionList = await GetListById(id);

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

        public async Task<TransmissionListEvent> ChangeEventPosition(int listId, int eventId, int previousPosition, int targetPosition)
        {
            TransmissionList transmissionList = await GetListById(listId);
            int foundEventIndex = transmissionList.GetEventPositionById(eventId);

            if (foundEventIndex != previousPosition)
            {
                string message = $"Unable to find list event with id {eventId} at position {previousPosition}";
                Logger.LogError(message);
                throw new InvalidPositionException(message, nameof(previousPosition));
            }

            if (targetPosition >= transmissionList.Events.Count)
            {
                string message = $"Unable to move list event with id {eventId} to target position {targetPosition}. Position is past the end of the list.";
                Logger.LogError(message);
                throw new InvalidPositionException(message, nameof(targetPosition));
            }

            TransmissionListEvent transmissionListEvent = transmissionList.Events[previousPosition];
            transmissionList.Events.RemoveAt(previousPosition);
            transmissionList.Events.Insert(targetPosition, transmissionListEvent);
            await DataLayer.AddUpdateTransmissionLists(transmissionList);

            int changePosition = Math.Min(previousPosition, targetPosition);
            ITransmissionListService transmissionListService = TransmissionListServiceStore.GetTransmissionListServiceByListId(listId);
            transmissionListService?.OnTransmissionListChanged(changePosition);
            return transmissionListEvent;
        }

        public async Task StopTransmissionList(int id)
        {
            TransmissionList transmissionList = await GetListById(id);

            ITransmissionListService transmissionListService = TransmissionListServiceStore.GetTransmissionListServiceByListId(id);
            if (transmissionListService == null)
            {
                string message = $"Unable to find list service for list with Id {id}";
                Logger.LogError(message);
                throw new ArgumentException(message, nameof(id));
            }

            transmissionListService.StopTransmissionList();
            await DataLayer.AddUpdateTransmissionLists(transmissionList);
        }

        public Task NextTransmissionList(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<TransmissionListEvent> UpdateEventDetails(int listId, int eventId, TransmissionListEventUpsertDTO listEventUpdate)
        {
            TransmissionList transmissionList = await GetListById(listId);
            int listEventPositionIndex = transmissionList.GetEventPositionById(eventId);

            TransmissionListEvent transmissionListEvent = transmissionList.Events[listEventPositionIndex];

            TransmissionListEvent createdEvent = TransmissionListEventFactory.BuildTransmissionListEvent(listEventUpdate.TimingData, listEventUpdate.Features, Channel.ChainConfiguration, DataLayer);
            
            transmissionListEvent.EventTimingStrategy = createdEvent.EventTimingStrategy;
            createdEvent.EventFeatures.ForEach((feature) =>
            {
                IEventFeature existingFeature = transmissionListEvent.EventFeatures.FirstOrDefault((oldFeature) => oldFeature.Uid == feature.Uid);
                if(existingFeature != null)
                {
                    feature.DeviceListEventId = existingFeature.DeviceListEventId;
                    feature.Device = existingFeature.Device;
                }
            });

            transmissionListEvent.EventFeatures = createdEvent.EventFeatures;
            await DataLayer.AddUpdateTransmissionLists(transmissionList);
            
            ITransmissionListService transmissionListService = TransmissionListServiceStore.GetTransmissionListServiceByListId(listId);
            transmissionListService?.OnTransmissionListChanged(listEventPositionIndex);
            return transmissionListEvent;
        }
    }
}