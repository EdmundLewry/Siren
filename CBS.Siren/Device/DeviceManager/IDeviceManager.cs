namespace CBS.Siren.Device
{
    public interface IDeviceManager
    {
        IDevice GetDevice(int id);
        void AddDevice(string name);
    }
}
