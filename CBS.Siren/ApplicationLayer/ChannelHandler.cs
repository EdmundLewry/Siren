using CBS.Siren.Data;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBS.Siren.Application
{
    public class ChannelHandler : IChannelHandler
    {
        private ILogger<ChannelHandler> Logger { get; }
        private IDataLayer DataLayer { get; }

        public ChannelHandler(ILogger<ChannelHandler> logger, IDataLayer dataLayer)
        {
            Logger = logger;
            DataLayer = dataLayer;
        }

        public async Task<IEnumerable<Channel>> GetAllChannels()
        {
            return await DataLayer.Channels();
        }
    }
}
