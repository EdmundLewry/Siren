using System.Collections.Generic;

namespace CBS.Siren.DTO
{
    public class TransmissionListEventUpsertDTO
    {
        public TimingStrategyUpsertDTO TimingData { get; set; }
        public List<ListEventFeatureUpsertDTO> Features { get; set; }
        public ListPositionDTO ListPosition { get; set; }
    }
}