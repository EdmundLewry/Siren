namespace CBS.Siren.DTO
{
    public class TransmissionListDTO
    {
        public int Id { get; set; }
        public PlaylistDTO SourceList { get; set; }
        public int EventCount { get; set; }
    }
}