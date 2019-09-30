using System;

namespace PBS.Siren
{
    /*
    A Channel List event is the combination of a particular Transmission Event and
    a particular set of devices required to fulfil the event.

    It allows us to have an understanding of what devices we expect to be used for
    an event in the future, so that we can perform validation based on the real device
    but without needing to interact with the device for every event in the list that would
    use it.
    */
    public class ChannelListEvent
    {
        public ChannelListEventState EventState { get; set; }
        //We may want more human readable identifiers
        public Guid Id { get; set; }


        //Should it reference a Transmission Event, or be a copy of it's data?
        public TransmissionEvent RelatedTransmissionEvent { get; set; }
        public IDevice Device { get; set; }

        public ChannelListEvent()
        {
            
        }
    }
}