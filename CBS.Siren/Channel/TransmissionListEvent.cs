using System;

namespace CBS.Siren
{
    /*
    A Transmission List event is the combination of a particular Playlist Event and
    a particular set of devices required to fulfil the event.

    It allows us to have an understanding of what devices we expect to be used for
    an event in the future, so that we can perform validation based on the real device
    but without needing to interact with the device for every event in the list that would
    use it.
    */
    public class TransmissionListEvent
    {
        public TransmissionListEventState EventState { get; set; }
        //We may want more human readable identifiers
        public Guid Id { get; set; }


        //Should it reference a Playlist Event, or be a copy of it's data?
        public PlaylistEvent RelatedPlaylistEvent { get; set; }
        public IDevice Device { get; set; }

        public TransmissionListEvent(PlaylistEvent PlaylistEvent, IDevice deviceForPlayout)
        {
            RelatedPlaylistEvent = PlaylistEvent;
            Device = deviceForPlayout;
            EventState = new TransmissionListEventState();
            Id = Guid.NewGuid();
        }

        public override String ToString()
        {
            //Would like to use Json for this
            return base.ToString() + ":\nId: " + Id.ToString() + "\nPlaylist Event: " + RelatedPlaylistEvent.ToString() + "\nDevice: " + Device.ToString();
        }
    }
}