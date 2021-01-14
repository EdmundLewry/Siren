using CBS.Siren.Data;
using CBS.Siren.Device;
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
        public IDeviceManager DeviceManager { get; }

        public ChannelHandler(ILogger<ChannelHandler> logger, IDataLayer dataLayer, IDeviceManager deviceManager)
        {
            Logger = logger;
            DataLayer = dataLayer;
            DeviceManager = deviceManager;
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

        public async Task<Channel> AddChannel(string channelName)
        {
            IEnumerable<Channel> existingChannels = await DataLayer.Channels();
            ValidateNewChannelDetails(channelName, existingChannels);

            Channel createdChannel = GenerateChannel(channelName, DeviceManager);
            List<Channel> channels = await DataLayer.AddUpdateChannels(createdChannel);
            if(channels.Count <= 0)
            {
                throw new ArgumentException($"Failed to create channel with name {channelName}", nameof(channelName));
            }

            return channels.First();
        }

        private void ValidateNewChannelDetails(string channelName, IEnumerable<Channel> existingChannels)
        {
            if (string.IsNullOrWhiteSpace(channelName))
            {
                throw new ArgumentException($"Channel must not be empty", nameof(channelName));
            }

            if (existingChannels.Any((channel) => channel.Name == channelName))
            {
                throw new ArgumentException($"Channel with name {channelName} already exists", nameof(channelName));
            }
        }

        private Channel GenerateChannel(string name, IDeviceManager deviceManager)
        {
            List<DeviceModel> deviceModels = DataLayer.Devices().Result.ToList();
            List<IDevice> devices = deviceModels.Select(model => deviceManager.GetDevice(model.Id)).ToList();
            VideoChain chainConfiguration = new VideoChain(devices);

            return new Channel
            {
                Name = name,
                ChainConfiguration = chainConfiguration
            };
        }
    }
}
