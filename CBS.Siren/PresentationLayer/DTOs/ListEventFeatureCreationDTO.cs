namespace CBS.Siren.DTO
{
    public class ListEventFeatureCreationDTO
    {
        public string FeatureType { get; set; }
        public PlayoutStrategyCreationDTO PlayoutStrategy { get; set; }
        public SourceStrategyCreationDTO SourceStrategy { get; set; }
        public int DeviceId { get; set; }
    }
}