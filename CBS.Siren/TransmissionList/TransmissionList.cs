using System.Collections.Generic;

namespace CBS.Siren
{
    /*
    A Transmission List is a list of Transmission Events which reference PLaylist Events
    and a specific Device or set of devices that are expected to be used to carry out
    the event. This list contains the entire Playlist List, and has the responsibility
    of performing cold validation of the events against where they're expected to playout
    given the current configuration and status of devices in the Channel's playout chain
    configuration.

    It also has the responsibility of generating the Device Lists for each device, which will
    be used to actually control the devices.
     */
    public class TransmissionList
    {
        public IPlaylist SourceList { get; set; }
        public IScheduler Scheduler { get; set; }
        public List<TransmissionListEvent> Events { get; }
        
        public TransmissionList(List<TransmissionListEvent> events, IPlaylist list, IScheduler listScheduler)
        {
            Scheduler = listScheduler;
            SourceList = list;
            Events = events;
        }
    }
}