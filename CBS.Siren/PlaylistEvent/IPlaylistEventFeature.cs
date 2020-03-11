namespace CBS.Siren
{
    public interface IPlaylistEventFeature
    {
        IPlayoutStrategy PlayoutStrategy { get; set; }
        ISourceStrategy SourceStrategy { get; set; }       
    }
}