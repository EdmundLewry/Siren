using CBS.Siren.Time;
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
        public TransmissionListEventState EventState { get; set; } = new TransmissionListEventState();

        public int Id { get; set; }

        public IEventTimingStrategy EventTimingStrategy { get; set; }
        public List<IEventFeature> EventFeatures { get; set; }

        public TimeSpan ExpectedDuration { get; set; }
        public DateTimeOffset ExpectedStartTime { get; set; }
        public DateTimeOffset? ActualStartTime { get; set; } = null;
        public DateTimeOffset? ActualEndTime { get; set; } = null;

        //I think this should just be a way to reference the related playlist event
        //There may not be a related event, so this could be null. We may choose to do
        //this with an id, but no reason not to store the event right now
        public PlaylistEvent RelatedPlaylistEvent { get; set; }

        public TransmissionListEvent(IEventTimingStrategy eventTiming, List<IEventFeature> features, PlaylistEvent PlaylistEvent = null)
        {
            RelatedPlaylistEvent = PlaylistEvent;
            EventFeatures = features;
            EventTimingStrategy = eventTiming;
        }

        public override string ToString()
        {
            string returnValue = base.ToString() +
                    $":\nId: {Id}" +
                    $"\nExpectedStartTime: {ExpectedStartTime.ToTimecodeString()}" +
                    $"\nExpectedDuration: {ExpectedDuration.ToTimecodeString()}" +
                    $"\nActualStartTime: {(ActualStartTime != null ? ActualStartTime?.ToTimecodeString() : "null")}" +
                    $"\nActualEndTime: {(ActualEndTime != null ? ActualEndTime?.ToTimecodeString() : "null")}" +
                    $"\nTimingStategy - {EventTimingStrategy?.ToString()}" +
                    $"\nRelated Playlist Event Id: {RelatedPlaylistEvent?.Id}";

            EventFeatures.ForEach((feature) => {
                returnValue += $"\nEventFeature - {feature}";
            });

            return returnValue;
        }
    }
}