namespace CBS.Siren.Device
{
    public class DeviceModel
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; }
        public DeviceProperties DeviceProperties { get; set; } = new DeviceProperties();
    }
}
