using CBS.Siren.Device;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CBS.Siren
{
    public class VideoPlaylistEventFeature : IEventFeature
    {
        public string FeatureType { get; } = "video";
        public IPlayoutStrategy PlayoutStrategy { get; set; }
        public ISourceStrategy SourceStrategy { get; set; }
        public IDevice Device { get; set; }
        public int? DeviceListEventId { get; set; }

        public VideoPlaylistEventFeature(IPlayoutStrategy playoutStrategy, ISourceStrategy sourceStrategy, IDevice device = null)
        {
            PlayoutStrategy = playoutStrategy;
            SourceStrategy = sourceStrategy;
            Device = device;
        }

        public VideoPlaylistEventFeature(IFeaturePropertiesFactory factory, MediaInstance mediaInstance, IDevice device = null) :
            this(factory.CreatePrimaryVideoPlayoutStrategy(), factory.CreateMediaSourceStrategy(mediaInstance), device)
        {
        }

        public override string ToString()
        {
            return "VideoPlaylistEventFeature:" +
            $"\nPlayout Strategy: {PlayoutStrategy}" +
            $"\nSource Strategy: {SourceStrategy}" + 
            $"\nDevice - {Device?.ToString()}" +
            $"\nDeviceListEvent - {DeviceListEventId}";
        }

        public virtual bool Equals([AllowNull] IEventFeature other)
        {
            return other is VideoPlaylistEventFeature &&
                    FeatureType == other.FeatureType &&
                    PlayoutStrategy.Equals(other.PlayoutStrategy) &&
                    SourceStrategy.Equals(other.SourceStrategy) &&
                    Device?.Model?.Name == other.Device?.Model?.Name;
        }

        public TimeSpan CalculateDuration()
        {
            //For our duration, we only care about the length of the Media Source Strategy
            return SourceStrategy.GetDuration();
        }
    }
}