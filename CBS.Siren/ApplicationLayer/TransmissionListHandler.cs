using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CBS.Siren.DTO;
using CBS.Siren.Data;
using CBS.Siren.Device;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace CBS.Siren.Application
{
    public class TransmissionListHandler : ITransmissionListHandler
    {
        public ILogger<TransmissionListHandler> Logger { get; }
        public IDataLayer DataLayer { get; }
        public Channel Channel { get; }

        public TransmissionListHandler(ILogger<TransmissionListHandler> logger, IDataLayer dataLayer)
        {
            Logger = logger;
            DataLayer = dataLayer;

            Channel = GenerateChannel(null);

            /* For this early stage we're just going to create a single transmission list to work on.
            This is because sat this stage of the application, it's not possible to add transmission lists
            to channels */
            InitializeTransmissionList();
        }

        private void InitializeTransmissionList()
        {
            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>(), null);
            DataLayer.AddUpdateTransmissionLists(transmissionList);
        }

        private Channel GenerateChannel(IDevice device)
        {
            List<IDevice> devices = new List<IDevice>() { device };
            VideoChain chainConfiguration = new VideoChain(devices);

            return new Channel(chainConfiguration);
        }

        public async Task<IEnumerable<TransmissionList>> GetAllLists()
        {
            return await DataLayer.TransmissionLists();
        }

        public async Task<IEnumerable<TransmissionListEvent>> GetListEvents(string id)
        {
            IEnumerable<TransmissionList> transmissionLists = await DataLayer.TransmissionLists();
            TransmissionList transmissionList = transmissionLists.ToList().FirstOrDefault(list => list.Id == id);

            if(transmissionList == null)
            {
                throw new ArgumentException($"Unable to find list with id: {id}", "id");
            }

            return transmissionList.Events;
        }

        public async Task<TransmissionListEvent> AddEvent(string id, TransmissionListEventCreationDTO listEvent)
        {
            IEnumerable<TransmissionList> transmissionLists = await DataLayer.TransmissionLists();
            TransmissionList transmissionList = transmissionLists.ToList().FirstOrDefault(list => list.Id == id);

            if(transmissionList == null)
            {
                throw new ArgumentException($"Unable to find list with id: {id}", "id");
            }

            TransmissionListEvent createdEvent = TransmissionListEventFactory.BuildTransmissionListEvent(listEvent.TimingData, new List<ListEventFeatureCreationDTO>(), null);
            return createdEvent;
        }
    }
}