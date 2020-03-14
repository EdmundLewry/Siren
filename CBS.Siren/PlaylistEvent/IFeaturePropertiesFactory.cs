namespace CBS.Siren
{
    public interface IFeaturePropertiesFactory
    {
        ISourceStrategy CreateMediaSourceStrategy(MediaInstance mediaInstance);
        IPlayoutStrategy CreatePrimaryVideoPlayoutStrategy();
    }
}
