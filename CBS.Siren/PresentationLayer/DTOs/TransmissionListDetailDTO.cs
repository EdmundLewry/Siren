using System.Collections.Generic;

namespace CBS.Siren.DTO
{
    public class TransmissionListDetailDTO
    {
        public int Id { get; set; }
        public PlaylistDTO SourceList { get; set; }
        public List<TransmissionListEventDTO> Events { get; set; }
        public string ListState { get; set; }
        public int? CurrentEventId { get; set; }
    }
}
