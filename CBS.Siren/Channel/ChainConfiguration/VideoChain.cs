using CBS.Siren.Device;
using System.Collections.Generic;

namespace CBS.Siren
{
    public class VideoChain : IVideoChain
    {
        public List<IDevice> ChainDevices { get; }
        public VideoChain(List<IDevice> devices)
        {
            ChainDevices = devices;
        }
    }
}