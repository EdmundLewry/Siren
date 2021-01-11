using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBS.Siren.Application
{
    public interface IChannelHandler
    {
        Task<IEnumerable<Channel>> GetAllChannels();
        Task<Channel> GetChannelById(int id);
        Task<Channel> AddChannel(string channelName);
    }
}
