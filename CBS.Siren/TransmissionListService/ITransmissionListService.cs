namespace CBS.Siren
{
    public interface ITransmissionListService : IDeviceListEventStatusChangeListener
    {
        TransmissionList TransmissionList { get; set; }
        void PlayTransmissionList();
        void NextTransmissionList();
        void StopTransmissionList();
        void OnTransmissionListChanged(int changeIndex = 0);
    }
}