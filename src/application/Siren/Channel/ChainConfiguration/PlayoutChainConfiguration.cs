using System.Collections.Generic;

namespace PBS.Siren
{
    public class PlayoutChainConfiguration : IPlayoutChainConfiguration
    {
        public List<IDevice> ChainDevices { get; }
        public PlayoutChainConfiguration(List<IDevice> devices)
        {
            ChainDevices = devices;
        }
    }
}