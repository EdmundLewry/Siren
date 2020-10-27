using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CBS.Siren.Data;
using CBS.Siren.Device;
using CBS.Siren.DTO;
using CBS.Siren.Time;

namespace CBS.Siren
{
    public static class TransmissionListEventFactory
    {
        public static TransmissionListEvent BuildTransmissionListEvent(TimingStrategyUpsertDTO timingData, List<ListEventFeatureUpsertDTO> featureData, IVideoChain videoChain, IDataLayer dataLayer)
        {
            IEventTimingStrategy timingStrategy = ConstructTimingStrategyFromType(timingData);
            List<IEventFeature> features = ConstructEventFeaturesFromList(featureData, videoChain, dataLayer);
            return new TransmissionListEvent(timingStrategy, features, null);
        }
        
        public static TransmissionListEvent BuildTransmissionListEvent(PlaylistEvent playlistEvent, IVideoChain videoChain, IDataLayer dataLayer)
        {
            IEventTimingStrategy timingStrategy = ConstructTimingStrategyFromType(playlistEvent.EventTimingStrategy);
            List<IEventFeature> features = ConstructEventFeaturesFromList(playlistEvent.EventFeatures, videoChain, dataLayer);
            return new TransmissionListEvent(timingStrategy, features, playlistEvent);
        }

        private static List<IEventFeature> ConstructEventFeaturesFromList(List<ListEventFeatureUpsertDTO> featureData, IVideoChain videoChain, IDataLayer dataLayer)
        {
            List<IEventFeature> features = new List<IEventFeature>();

            featureData.ForEach((featureItem) => {
                features.Add(ConstructEventFeatureFromType(featureItem, videoChain, dataLayer));
            });

            return features;
        }
        
        private static List<IEventFeature> ConstructEventFeaturesFromList(IEnumerable<IEventFeature> featureData, IVideoChain videoChain, IDataLayer dataLayer)
        {
            List<IEventFeature> features = new List<IEventFeature>();

            featureData.ToList().ForEach((featureItem) => {
                features.Add(ConstructEventFeatureFromType(featureItem, videoChain));
            });

            return features;
        }

        private static IEventFeature ConstructEventFeatureFromType(ListEventFeatureUpsertDTO feature, IVideoChain videoChain, IDataLayer dataLayer)
        {
            IDevice deviceToPlayOn = FindDevice(videoChain);

            return feature.FeatureType switch
            {
                "video" => new VideoPlaylistEventFeature(ConstructPlayoutStrategyFromType(feature.PlayoutStrategy),
                                                        ConstructSourceStrategyFromType(feature.SourceStrategy, deviceToPlayOn, dataLayer),
                                                        feature.Duration.ConvertTimecodeStringToTimeSpan(),
                                                        deviceToPlayOn),
                _ => null
            };
        }

        private static IEventFeature ConstructEventFeatureFromType(IEventFeature feature, IVideoChain videoChain)
        {
            IDevice deviceToPlayOn = FindDevice(videoChain);

            return feature.FeatureType switch
            {
                "video" => new VideoPlaylistEventFeature(ConstructPlayoutStrategyFromType(feature.PlayoutStrategy),
                                                        ConstructSourceStrategyFromType(feature.SourceStrategy),
                                                        feature.Duration,
                                                        deviceToPlayOn),
                _ => null
            };
        }

        private static IDevice FindDevice(IVideoChain videoChain)
        {
            //Currently have no way to decide what device should go where. So we just put the first one in
            //Eventually we'll pull this from the Device to event mapping
            IDevice deviceToPlayOn = null;
            if (videoChain != null && videoChain.ChainDevices.Any())
            {
                deviceToPlayOn = videoChain.ChainDevices.FirstOrDefault();
            }

            return deviceToPlayOn;
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

        private static IEventTimingStrategy ConstructTimingStrategyFromType(TimingStrategyUpsertDTO timingData)
        {
            return timingData.StrategyType switch
            {
                "fixed" => new FixedStartEventTimingStrategy(timingData.TargetStartTime.ConvertTimecodeStringToDateTime()),
                "sequential" => new SequentialStartEventTimingStrategy(),
                _ => null
            };
        }

        private static IPlayoutStrategy ConstructPlayoutStrategyFromType(IPlayoutStrategy playoutStrategy)
        {
            PlayoutStrategyCreationDTO creationDTO = new PlayoutStrategyCreationDTO() {StrategyType = playoutStrategy.StrategyType};
            return ConstructPlayoutStrategyFromType(creationDTO);
        }
        
        private static IPlayoutStrategy ConstructPlayoutStrategyFromType(PlayoutStrategyCreationDTO playoutStrategy)
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
        
        private static ISourceStrategy ConstructSourceStrategyFromType(SourceStrategyCreationDTO sourceStrategy, IDevice device, IDataLayer dataLayer)
        {
            return sourceStrategy.StrategyType switch
            {
                "mediaSource" => new MediaSourceStrategy(GetMediaInstanceByNameAsync(sourceStrategy.MediaName, device, dataLayer).Result, 
                                                         sourceStrategy.SOM.ConvertTimecodeStringToTimeSpan(), 
                                                         sourceStrategy.EOM.ConvertTimecodeStringToTimeSpan()),
                _ => null
            };
        }

        private static async Task<MediaInstance> GetMediaInstanceByNameAsync(string mediaName, IDevice device, IDataLayer dataLayer)
        {
            var instances = await dataLayer.MediaInstances();
            return instances.FirstOrDefault((instance)=> instance.Name == mediaName);
        }
    }
}