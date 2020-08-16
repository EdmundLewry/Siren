using System;

namespace CBS.Siren.DTO
{
    public class TransmissionListEventDTO
    {
        public string EventState { get; set; }
        public int Id { get; set; }
        public string EventTimingStrategy { get; set; }
        public int EventFeatureCount { get; set; }
        public string ExpectedDuration { get; set; }
        public string ExpectedStartTime { get; set; }
        public string RelatedPlaylistEvent { get; set; }
        public int RelatedDeviceListEventCount { get; set; }
    }
}