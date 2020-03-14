using System;
using System.Collections.Generic;

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

        public IEventTimingStrategy EventTimingStrategy { get; set; }
        public List<IEventFeature> EventFeatures { get; set; }

        //This should be in timecode
        public int ExpectedDuration { get; set; } //Currently in frames (assuming 25FPS)
        //Should be in timecode
        public DateTime ExpectedStartTime { get; set; }

        //I think this should just be a way to reference the related playlist event
        //There may not be a related event, so this could be null. We may choose to do
        //this with an id, but no reason not to store the event right now
        public PlaylistEvent RelatedPlaylistEvent { get; set; }
        public IDevice Device { get; set; }

        public TransmissionListEvent(IDevice deviceForPlayout, IEventTimingStrategy eventTiming, List<IEventFeature> features, PlaylistEvent PlaylistEvent = null)
        {
            RelatedPlaylistEvent = PlaylistEvent;
            EventFeatures = features;
            Device = deviceForPlayout;
            EventTimingStrategy = eventTiming;
            EventState = new TransmissionListEventState();
            Id = Guid.NewGuid();
        }

        public override String ToString()
        {
            string returnValue =  base.ToString() + 
                    $":\nId: {Id}" +
                    $"\nExpectedStartTime: {ExpectedStartTime} ExpectedDuration: {ExpectedDuration}" +
                    $"\nTimingStategy: {EventTimingStrategy.ToString()}" +
                    $"\nPlaylist Event: {RelatedPlaylistEvent.Id}" + 
                    $"\nDevice: {Device?.ToString()}";

            EventFeatures.ForEach((feature) => {
                returnValue = returnValue + $"\nEventFeature: {feature.ToString()}";
            });

            return returnValue;
        }
    }
}