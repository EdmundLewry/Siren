using System;

namespace CBS.Siren
{
    public interface IEventFeature : IEquatable<IEventFeature>
    {
        string FeatureType { get; }
        IPlayoutStrategy PlayoutStrategy { get; set; }
        ISourceStrategy SourceStrategy { get; set; }
        
        int CalculateDuration();
    }
}