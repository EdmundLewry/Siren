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
        public String ValidationStatus { get; set; }

        //We may want more human readable identifiers
        public Guid Id { get; set; }
        
        public PlaylistEvent(IEnumerable<IEventFeature> features, IEventTimingStrategy timingStrategy)
        {
            EventFeatures = features;
            EventTimingStrategy = timingStrategy;
            Id = Guid.NewGuid();
        }

        public override string ToString()
        {
            string returnValue =  $"{base.ToString()}" + 
                    $"\nId: {Id.ToString()}" +
                    $"\nTimingStategy: {EventTimingStrategy.ToString()}";
            
            foreach(IEventFeature feature in EventFeatures)
            {
                returnValue += $"\nEvent Feature: {feature.ToString()}";
            }

            return returnValue;
        }
    }
}
