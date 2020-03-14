using System;
using System.Diagnostics.CodeAnalysis;

namespace CBS.Siren
{
    public class VideoPlaylistEventFeature : IEventFeature
    {
        public string FeatureType { get; } = "video";
        public IPlayoutStrategy PlayoutStrategy { get; set; }
        public ISourceStrategy SourceStrategy { get; set; }
        private IFeaturePropertiesFactory PropertiesFactory { get; set; }

        public VideoPlaylistEventFeature(IFeaturePropertiesFactory factory, MediaInstance mediaInstance)
        {
            PropertiesFactory = factory;
            PlayoutStrategy = PropertiesFactory.CreatePrimaryVideoPlayoutStrategy();
            SourceStrategy = PropertiesFactory.CreateMediaSourceStrategy(mediaInstance);
        }
        
        public VideoPlaylistEventFeature(IFeaturePropertiesFactory factory, IPlayoutStrategy playoutStrategy, ISourceStrategy sourceStrategy)
        {
            PropertiesFactory = factory;
            PlayoutStrategy = playoutStrategy;
            SourceStrategy = sourceStrategy;
        }

        public override string ToString()
        {
            return "VideoPlaylistEventFeature:" +
            $"\nPlayout Strategy: {PlayoutStrategy.ToString()}" +
            $"\nSource Strategy: {SourceStrategy.ToString()}";
        }

        public bool Equals([AllowNull] IEventFeature other)
        {
            return other is VideoPlaylistEventFeature videoEvent &&
                    FeatureType == other.FeatureType &&
                    PlayoutStrategy.Equals(other.PlayoutStrategy) &&
                    SourceStrategy.Equals(other.SourceStrategy);
        }
    }
}