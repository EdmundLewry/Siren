using System.Collections.Generic;

namespace PBS.Siren
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