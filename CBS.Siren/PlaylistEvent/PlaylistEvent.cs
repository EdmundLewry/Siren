using CBS.Siren.Utilities;
using System;
using System.Collections.Generic;

namespace CBS.Siren
{
    /*
    A playlist event is an item in a Playlist that defines what should be played out,
    for how long, when it should start, where in the channel layout it should be displayed,
    and validates that that data makes sense against the source being represented.
     */
    public class PlaylistEvent
    {
        public IEnumerable<IEventFeature> EventFeatures { get; set; }
        public IEventTimingStrategy EventTimingStrategy { get; set; }

        public bool IsValid { get; set; }
        public string ValidationStatus { get; set; }

        public int Id { get; set; }
        
        public PlaylistEvent(IEnumerable<IEventFeature> features, IEventTimingStrategy timingStrategy)
        {
            EventFeatures = features;
            EventTimingStrategy = timingStrategy;
            Id = IdFactory.NextPlaylistEventId();
        }

        public override string ToString()
        {
            string returnValue =  $"{base.ToString()}" + 
                    $"\nId: {Id}" +
                    $"\nTimingStategy: {EventTimingStrategy}";
            
            foreach(IEventFeature feature in EventFeatures)
            {
                returnValue += $"\nEvent Feature: {feature}";
            }

            return returnValue;
        }
    }
}
