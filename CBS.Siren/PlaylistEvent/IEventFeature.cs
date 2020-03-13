namespace CBS.Siren
{
    public interface IEventFeature
    {
        IPlayoutStrategy PlayoutStrategy { get; set; }
        ISourceStrategy SourceStrategy { get; set; }       
    }
}