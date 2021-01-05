using CBS.Siren.DTO;
using System.Collections.Generic;

namespace CBS.Siren.PresentationLayer.DTOs
{
    public class ChannelDetailsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<TransmissionListDTO> TransmissionLists { get; set; }
    }
}
