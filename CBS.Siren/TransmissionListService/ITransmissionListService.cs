namespace CBS.Siren
{
    public interface ITransmissionListService : IDeviceListEventStatusChangeListener
    {
        TransmissionList TransmissionList { get; set; }
        void PlayTransmissionList();
    }
}