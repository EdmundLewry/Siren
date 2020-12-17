using CBS.Siren.Data;
using CBS.Siren.DataLayer;

namespace CBS.Siren.Test
{
    public class ChannelDataLayerInitializer : IDataLayerInitializer
    {
        private readonly IDataLayer _dataLayer;

        public ChannelDataLayerInitializer(IDataLayer dataLayer)
        {
            _dataLayer = dataLayer;
        }

        public void Seed()
        {
            _dataLayer.AddUpdateChannels(new Channel() { Name = "TestChannel" });
        }
    }
}
