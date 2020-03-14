using System;
using System.Collections.Generic;

namespace CBS.Siren
{
    public static class TransmissionListBuilder
    {
        public static TransmissionList BuildFromPlaylist(IPlaylist list)
        {
            List<TransmissionListEvent> createdEvents = new List<TransmissionListEvent>();
            list.Events.ForEach((playlistEvent) => createdEvents.Add(GenerateTransmissionEvent(playlistEvent)));

            return new TransmissionList(createdEvents, list, new SimpleChannelScheduler());
        }

        private static TransmissionListEvent GenerateTransmissionEvent(PlaylistEvent playlistEvent)
        {
            List<IEventFeature> features = new List<IEventFeature>();

            //I suspect Event Features in the playlist will be just data later on, so just storing the type for now
            foreach(IEventFeature feature in playlistEvent.EventFeatures)
            {
                features.Add(ConstructEventFeatureFromType(feature));
            }

            IEventTimingStrategy eventTimingStrategy = ConstructTimingStrategyFromType(playlistEvent.EventTimingStrategy);
            return new TransmissionListEvent(null, eventTimingStrategy, features, playlistEvent);
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