namespace CBS.Siren
{
    public class VideoPlaylistEventFeature : IPlaylistEventFeature
    {
        public IPlayoutStrategy PlayoutStrategy { get; set; }
        public ISourceStrategy SourceStrategy { get; set; }
        private IFeaturePropertiesFactory PropertiesFactory { get; set; }

        public VideoPlaylistEventFeature(IFeaturePropertiesFactory factory, MediaInstance mediaInstance)
        {
            PropertiesFactory = factory;
            PlayoutStrategy = PropertiesFactory.CreatePrimaryVideoPlayoutStrategy();
            SourceStrategy = PropertiesFactory.CreateMediaSourceStrategy(mediaInstance);
        }
    }
}