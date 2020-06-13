using System.Collections.Generic;
using System.Linq;
using CBS.Siren.Device;
using System.Text.Json;

namespace CBS.Siren
{
    public static class TransmissionListEventFactory
    {
        public static TransmissionListEvent BuildTransmissionListEvent(List<JsonElement> featureData, IVideoChain videoChain)
        {
            IEventTimingStrategy timingStrategy;
            List<IEventFeature> features = new List<IEventFeature>();
            return new TransmissionListEvent(timingStrategy, features, null);
        }

        private static IEventFeature ConstructEventFeatureFromType(IEventFeature feature, IVideoChain videoChain)
        {
            //Currently have no way to decide what device should go where. So we just put the first one in
            //Eventually we'll pull this from the Device to event mapping
            IDevice deviceToPlayOn = null;
            if (videoChain != null && videoChain.ChainDevices.Any())
            {
                deviceToPlayOn = videoChain.ChainDevices.FirstOrDefault();
            }

            return feature.FeatureType switch
            {
                "video" => new VideoPlaylistEventFeature(ConstructPlayoutStrategyFromType(feature.PlayoutStrategy), 
                                                        ConstructSourceStrategyFromType(feature.SourceStrategy),
                                                        deviceToPlayOn),
                _ => null
            };
        }

        private static IEventTimingStrategy ConstructTimingStrategyFromType(IEventTimingStrategy eventTimingStrategy)
        {
            return eventTimingStrategy.StrategyType switch
            {
                "fixed" => new FixedStartEventTimingStrategy(eventTimingStrategy),
                "sequential" => new SequentialStartEventTimingStrategy(eventTimingStrategy),
                _ => null
            };
        }

        private static IPlayoutStrategy ConstructPlayoutStrategyFromType(IPlayoutStrategy playoutStrategy)
        {
            return playoutStrategy.StrategyType switch
            {
                "primaryVideo" => new PrimaryVideoPlayoutStrategy(),
                _ => null
            };
        }

        private static ISourceStrategy ConstructSourceStrategyFromType(ISourceStrategy sourceStrategy)
        {
            return sourceStrategy.StrategyType switch
            {
                "mediaSource" => new MediaSourceStrategy(sourceStrategy),
                _ => null
            };
        }
    }
}