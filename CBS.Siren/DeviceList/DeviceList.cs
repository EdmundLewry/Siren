using System.Collections.Generic;

namespace CBS.Siren
{
    /*
    The Device List is a collection of Playout Events which define what will functionality
    will be required in the most immediate future. Final validation of the event with the devices that
    are going to act on those events is handled here. This is also where manual control actions are performed.

    A device should be passed this list and then act based on the list data as it needs to to ensure that
    the event is actioned at the correct time.
    */
    public class DeviceList
    {
        public List<DeviceListEvent> Events { get; set; }
        
        public DeviceList(List<DeviceListEvent> events)
        {
            Events = events;
        }
    }
}