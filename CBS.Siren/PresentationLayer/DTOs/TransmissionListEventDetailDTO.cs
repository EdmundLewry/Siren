using System.Collections.Generic;

namespace CBS.Siren.DTO
{
    public class TransmissionListEventDetailDTO
    {
        public string EventState { get; set; }
        public int Id { get; set; }
        public string ExpectedDuration { get; set; }
        public string ExpectedStartTime { get; set; }
        public string ActualStartTime { get; set; }
        public string ActualEndTime { get; set; }
        public int RelatedDeviceListEventCount { get; set; }
        public TimingStrategyDTO EventTimingStrategy { get; set; }
        public List<ListEventFeatureDTO> EventFeatures { get; set; }
        public int? RelatedPlaylistEventId { get; set; }
    }
}