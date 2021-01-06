using CBS.Siren.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<Channel> GetChannelById(int id)
        {
            IEnumerable<Channel> channels = await DataLayer.Channels();

            Channel retrievedChannel = channels.FirstOrDefault(channel => channel.Id == id);

            if (retrievedChannel == null)
            {
                Logger.LogError($"Unable to find channel with Id {id}");
                throw new ArgumentException($"Unable to find channel with id: {id}", "id");
            }

            return retrievedChannel;
        }
    }
}
