using System;

namespace CBS.Siren.DTO
{
    public class TransmissionListEventDTO
    {
        public string EventState { get; set; }
        public Guid Id { get; set; }
        public string EventTimingStrategy { get; set; }
        public int EventFeatureCount { get; set; }
        public TimeSpan ExpectedDuration { get; set; }
        public DateTime ExpectedStartTime { get; set; }
        public string RelatedPlaylistEvent { get; set; }
        public int RelatedDeviceListEventCount { get; set; }
    }
}