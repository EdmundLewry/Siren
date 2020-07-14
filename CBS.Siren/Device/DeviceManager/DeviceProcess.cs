using System.Threading;

namespace CBS.Siren.Device
{
    public class DeviceProcess
    {
        public IDevice Device { get; }
        public CancellationTokenSource CancellationTokenSource { get; }
        public Thread DeviceThread { get; }

        public DeviceProcess(IDevice device, CancellationTokenSource cancellationTokenSource, Thread thread)
        {
            Device = device;
            CancellationTokenSource = cancellationTokenSource;
            DeviceThread = thread;
        }
    }
}
