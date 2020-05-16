using System.Collections.Generic;
using System.Threading.Tasks;
using CBS.Siren.Data;
using CBS.Siren.Device;
using Microsoft.Extensions.Logging;

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
    }
}