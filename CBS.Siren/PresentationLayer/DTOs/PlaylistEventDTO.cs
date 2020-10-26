namespace CBS.Siren.DTO
{
    public class PlaylistEventDTO
    {
        public int Id { get; set; }
        public TimingStrategyDTO EventTimingStrategy { get; set; }
        public int FeatureCount { get; set; }
    }
}