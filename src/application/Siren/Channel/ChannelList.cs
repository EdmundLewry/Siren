using System.Collections.Generic;

namespace PBS.Siren
{
    /*
    A Channel List is a list of Channel Events which reference Transmission Events
    and a specific Device or set of devices that are expected to be used to carry out
    the event. This list contains the entire Transmission List, and has the responsibility
    of performing cold validation of the events against where they're expected to playout
    given the current configuration and status of devices in the Channel's playout chain
    configuration.

    It also has the responsibility of generating the Playout Lists for each device, which will
    be used to actually control the devices.
     */
    public class ChannelList
    {
        public List<ChannelListEvent> Events { get; }
        public IDevice Device { get; set; }
        
        public ChannelList()
        {
            
        }
    }
}