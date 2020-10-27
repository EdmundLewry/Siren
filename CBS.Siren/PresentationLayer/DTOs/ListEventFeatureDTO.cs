namespace CBS.Siren.DTO
{
    public class ListEventFeatureDTO
    {
        public string FeatureType { get; set; }
        public PlayoutStrategyDTO PlayoutStrategy { get; set; }
        public SourceStrategyDTO SourceStrategy { get; set; }
        public DeviceDTO Device { get; set; }
        public string Duration { get; set; }
        public int? DeviceListEventId { get; set; }

    }
}