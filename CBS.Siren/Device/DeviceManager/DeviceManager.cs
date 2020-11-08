using CBS.Siren.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.Siren.Device
{
    public class DeviceManager : IDeviceManager, IDisposable
    {
        public IDataLayer DataLayer { get; }
        public ILogger<DeviceManager> Logger { get; }
        public ILoggerFactory LoggerFactory { get; }
        public IDeviceFactory DeviceFactory { get; } 
        public IDeviceListEventStore DeviceListEventStore { get; }
        public Dictionary<int, DeviceProcess> Devices { get; } = new Dictionary<int, DeviceProcess>();

        public DeviceManager(IDataLayer dataLayer, ILogger<DeviceManager> logger, ILoggerFactory loggerFactory, IDeviceFactory deviceFactory, IDeviceListEventStore deviceListEventStore)
        {
            DataLayer = dataLayer;
            Logger = logger;
            LoggerFactory = loggerFactory;
            DeviceFactory = deviceFactory;
            DeviceListEventStore = deviceListEventStore;

            ConstructExistingDevices();
        }

        private void ConstructExistingDevices()
        {
            List<DeviceModel> models = DataLayer.Devices().Result.ToList();
            models.ForEach(model =>
            {
                CreateDevice(model);
            });
        }

        public void AddDevice(string name, DeviceProperties deviceProperties = null)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name parameter cannot be null or empty", "name");
            }

            DeviceModel model = StoreDevice(name, deviceProperties).Result;
            CreateDevice(model);
        }

        public async Task<DeviceModel> StoreDevice(string name, DeviceProperties deviceProperties = null)
        {
            DeviceModel deviceModel = new DeviceModel() { Name = name, DeviceProperties = deviceProperties ?? new DeviceProperties() };
            List<DeviceModel> addedModels = await DataLayer.AddUpdateDevices(deviceModel);

            return addedModels[0];
        }

        private async Task StartDeviceThread(object deviceProcess)
        {
            //Feels bad man =(
            DeviceProcess process = deviceProcess as DeviceProcess;
            await process.Device.Run(process.CancellationTokenSource.Token);
        }

        private void CreateDevice(DeviceModel deviceModel)
        {
            IDevice device = DeviceFactory.CreateDemoDevice(deviceModel, LoggerFactory, DeviceListEventStore);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Thread deviceThread = new Thread(async (deviceProcess) => await StartDeviceThread(deviceProcess));

            DeviceProcess deviceProcess = new DeviceProcess(device, cancellationTokenSource, deviceThread);
            int id = deviceModel.Id;
            Devices.Add(id, deviceProcess);
            deviceProcess.DeviceThread.Start(Devices[id]);
        }

        public IDevice GetDevice(int id)
        {
            if(!Devices.ContainsKey(id))
            {
                throw new ArgumentException("There is no device available with the given id", "id");
            }
            return Devices[id].Device;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach(DeviceProcess deviceProcess in Devices.Values)
                    {
                        deviceProcess.CancellationTokenSource.Cancel();
                        deviceProcess.DeviceThread.Join();
                        deviceProcess.Device.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
             GC.SuppressFinalize(this);
        }
        #endregion
    }
}
