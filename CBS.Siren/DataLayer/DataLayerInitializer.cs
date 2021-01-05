using CBS.Siren.Data;
using System;
using System.Collections.Generic;

namespace CBS.Siren.DataLayer
{
    public class DataLayerInitializer : IDataLayerInitializer
    {
        private readonly IDataLayer _dataLayer;

        public DataLayerInitializer(IDataLayer dataLayer)
        {
            _dataLayer = dataLayer;
        }

        public void Seed()
        {
            _dataLayer.AddUpdateMediaInstances(new MediaInstance("TestInstance", new TimeSpan(0, 0, 30)));

            _dataLayer.AddUpdateChannels(new Channel() { Name = "TestChannel" });

            /* For this early stage we're just going to create a single transmission list to work on.
            This is because sat this stage of the application, it's not possible to add transmission lists
            to channels */
            InitializeTransmissionList(_dataLayer);
        }

        private void InitializeTransmissionList(IDataLayer dataLayer)
        {
            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>(), null);

            dataLayer.AddUpdateTransmissionLists(transmissionList);
        }
    }
}
