namespace CBS.Siren.PresentationLayer.DTOs
{
    public class TransmissionListEventPositionChangeDTO
    {
        public int TransmissionListId { get; set; }
        public int TransmissionListEventId { get; set; }
        public int PreviousPositionIndex { get; set; }
        public int TargetPositionIndex { get; set; }
    }
}
