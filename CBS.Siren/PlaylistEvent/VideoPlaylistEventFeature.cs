using CBS.Siren.Device;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CBS.Siren
{
    public class VideoPlaylistEventFeature : IEventFeature
    {
        public Guid? Uid { get; set; }
        public string FeatureType { get; } = "video";
        public IPlayoutStrategy PlayoutStrategy { get; set; }
        public ISourceStrategy SourceStrategy { get; set; }
        public IDevice Device { get; set; }
        public int? DeviceListEventId { get; set; }
        public TimeSpan Duration { get; set; }

        public VideoPlaylistEventFeature(Guid uid, IPlayoutStrategy playoutStrategy, ISourceStrategy sourceStrategy, TimeSpan duration, IDevice device = null)
        {
            Uid = uid;
            PlayoutStrategy = playoutStrategy;
            SourceStrategy = sourceStrategy;
            Device = device;
            Duration = duration;
        }

        public VideoPlaylistEventFeature(Guid uid, IFeaturePropertiesFactory factory, MediaInstance mediaInstance, TimeSpan? duration = null, IDevice device = null) :
            this(uid, factory.CreatePrimaryVideoPlayoutStrategy(), factory.CreateMediaSourceStrategy(mediaInstance), duration ?? mediaInstance.Duration, device)
        {
        }

        public override string ToString()
        {
            return "VideoPlaylistEventFeature:" +
            $"\nUid: {Uid}" +
            $"\nPlayout Strategy: {PlayoutStrategy}" +
            $"\nSource Strategy: {SourceStrategy}" + 
            $"\nDevice - {Device?.ToString()}" +
            $"\nDeviceListEvent - {DeviceListEventId}";
        }

        public virtual bool Equals([AllowNull] IEventFeature other)
        {
            return other is VideoPlaylistEventFeature &&
                    Uid == other.Uid &&
                    FeatureType == other.FeatureType &&
                    PlayoutStrategy.Equals(other.PlayoutStrategy) &&
                    SourceStrategy.Equals(other.SourceStrategy) &&
                    Device?.Model?.Name == other.Device?.Model?.Name &&
                    Duration == other.Duration;
        }
    }
}