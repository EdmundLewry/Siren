namespace CBS.Siren
{
    public interface ITransmissionListService : IDeviceListEventStatusChangeListener
    {
        public TransmissionList TransmissionList { get; set; }
        void PlayTransmissionList();
    }
}