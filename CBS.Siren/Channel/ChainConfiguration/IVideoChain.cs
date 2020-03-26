using CBS.Siren.Device;
using System.Collections.Generic;

namespace CBS.Siren
{
    /*
    A Playout Chain Configuration is the specific set of devices and their inter-connections
    which are going to be used to play content for a channel. 

    It is used, in combination with a Transmission List, to define what which devices will be used
    to enact each individual Transmission Event.
    */
    public interface IVideoChain
    {
        List<IDevice> ChainDevices { get; }
    }
}