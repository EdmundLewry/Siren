namespace CBS.Siren.DTO
{
    public class ListEventFeatureUpsertDTO
    {
        public string Uid { get; set; }
        public string FeatureType { get; set; }
        public PlayoutStrategyCreationDTO PlayoutStrategy { get; set; }
        public SourceStrategyCreationDTO SourceStrategy { get; set; }
        public string Duration { get; set; }
    }
}