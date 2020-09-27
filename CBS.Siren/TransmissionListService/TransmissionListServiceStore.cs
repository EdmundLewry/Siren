using CBS.Siren.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace CBS.Siren
{
    public class TransmissionListServiceStore : ITransmissionListServiceStore
    {
        private Dictionary<int, ITransmissionListService> AvailableServices { get; set; } = new Dictionary<int, ITransmissionListService>();
        private IServiceProvider ServiceProvider { get; }

        public TransmissionListServiceStore(IDataLayer dataLayer, IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;

            List<TransmissionList> availableLists = dataLayer.TransmissionLists().Result.ToList();
            availableLists.ForEach(list => CreateTransmissionListService(list));
        }

        private void CreateTransmissionListService(TransmissionList list)
        {
            ITransmissionListService transmissionListService = ServiceProvider.GetService<ITransmissionListService>();
            transmissionListService.TransmissionList = list;
            AvailableServices.Add(list.Id, transmissionListService);
        }

        public ITransmissionListService GetTransmissionListServiceByListId(int transmissionListId)
        {
            return AvailableServices.GetValueOrDefault(transmissionListId);
        }

        //TODO: Still need to make it so that we can add list service part way through
    }
}
