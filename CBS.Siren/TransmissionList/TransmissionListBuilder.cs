using System.Collections.Generic;
using System.Linq;

namespace CBS.Siren
{
    public static class TransmissionListBuilder
    {
        public static TransmissionList BuildFromPlaylist(IPlaylist list, IVideoChain videoChain)
        {
            List<TransmissionListEvent> createdEvents = new List<TransmissionListEvent>();
            list.Events.ForEach((playlistEvent) => createdEvents.Add(GenerateTransmissionEvent(playlistEvent, videoChain)));

            return new TransmissionList(createdEvents, list);
        }

        private static TransmissionListEvent GenerateTransmissionEvent(PlaylistEvent playlistEvent, IVideoChain videoChain)
        {
            List<IEventFeature> features = new List<IEventFeature>();

            //I suspect Event Features in the playlist will be just data later on, so just storing the type for now
            foreach(IEventFeature feature in playlistEvent.EventFeatures)
            {
                features.Add(ConstructEventFeatureFromType(feature));
            }

            //Currently have no way to decide what device should go where. So we just put the first one in
            //Eventually we'll pull this from the Device to event mapping
            IDevice deviceToPlayOn = null;
            if(videoChain != null && videoChain.ChainDevices.Any())
            {
                deviceToPlayOn = videoChain.ChainDevices.FirstOrDefault();
            }

            IEventTimingStrategy eventTimingStrategy = ConstructTimingStrategyFromType(playlistEvent.EventTimingStrategy);
            return new TransmissionListEvent(deviceToPlayOn, eventTimingStrategy, features, playlistEvent);
        }

        private static IEventFeature ConstructEventFeatureFromType(IEventFeature feature)
        {
            return feature.FeatureType switch
            {
                "video" => new VideoPlaylistEventFeature(new FeaturePropertiesFactory(), 
                                                        ConstructPlayoutStrategyFromType(feature.PlayoutStrategy), 
                                                        ConstructSourceStrategyFromType(feature.SourceStrategy)),
                _ => null
            };
        }

        private static IEventTimingStrategy ConstructTimingStrategyFromType(IEventTimingStrategy eventTimingStrategy)
        {
            return eventTimingStrategy.StrategyType switch
            {
                "fixed" => new FixedStartEventTimingStrategy(eventTimingStrategy),
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