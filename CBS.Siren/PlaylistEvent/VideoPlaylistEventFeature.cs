namespace CBS.Siren
{
    public class VideoPlaylistEventFeature : IPlaylistEventFeature
    {
        public IPlayoutStrategy PlayoutStrategy { get; set; }
        public ISourceStrategy SourceStrategy { get; set; }

        public VideoPlaylistEventFeature(IPlayoutStrategy playoutStrategy, ISourceStrategy sourceStrategy)
        {
            PlayoutStrategy = playoutStrategy;
            SourceStrategy = sourceStrategy;
        }
    }
}