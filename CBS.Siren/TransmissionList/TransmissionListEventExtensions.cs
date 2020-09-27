namespace CBS.Siren
{
    public static class TransmissionListEventExtensions
    {
        public static bool HasCompleted(this TransmissionListEvent transmissionEvent)
        {
            return transmissionEvent.ActualEndTime.HasValue;
        }

        public static bool HasStartedPlayingOut(this TransmissionListEvent transmissionEvent)
        {
            return transmissionEvent.ActualStartTime.HasValue;
        }
    }
}
