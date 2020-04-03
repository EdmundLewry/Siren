namespace CBS.Siren
{
    /*
        A Transmission List Event State is the changeable data that relates to an individual Transmission List Event,
        such as whether it's currently valid, it's current playout state (is it cued? playing?) and so on.
    */
    public class TransmissionListEventState
    {
        public enum Status
        {
            UNSCHEDULED,
            SCHEDULED,
            CUEING,
            CUED,
            PLAYING,
            PLAYED
        }

        public Status CurrentStatus { get; set; } = Status.UNSCHEDULED;
    }
}