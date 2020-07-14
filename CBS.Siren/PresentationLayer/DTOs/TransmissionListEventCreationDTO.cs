using System.Collections.Generic;

namespace CBS.Siren.DTO
{
    public class TransmissionListEventCreationDTO
    {
        public TimingStrategyCreationDTO TimingData {get; set;}
        public List<ListEventFeatureCreationDTO> Features {get; set;}
    }
}