namespace CBS.Siren
{
    public class FeaturePropertiesFactory : IFeaturePropertiesFactory
    {

        public IPlayoutStrategy CreatePrimaryVideoPlayoutStrategy()
        {
            return new PrimaryVideoPlayoutStrategy();
        }

        public ISourceStrategy CreateMediaSourceStrategy(MediaInstance mediaInstance)
        {
            return new MediaSourceStrategy(mediaInstance, 0, mediaInstance.Duration);
        }
    }
}
