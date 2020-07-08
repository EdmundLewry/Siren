using CBS.Siren.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CBS.Siren.Device
{
    public class DeviceManager : IDeviceManager, IDisposable
    {
        /*On Creation, takes the data layer and creates a new Device process for each device*/
        /*Has AddDevice functionality which adds a device to the data layer and then creates a process in memory for that device*/
        /*Has GetDeviceById functionality which returns the device object*/
        public IDataLayer DataLayer { get; }
        public ILogger Logger { get; }
        public ILoggerFactory LoggerFactory { get; }
        public IDeviceFactory DeviceFactory { get; }
        public Dictionary<int, DeviceProcess> Devices { get; } = new Dictionary<int, DeviceProcess>();

        public DeviceManager(IDataLayer dataLayer, ILogger logger, ILoggerFactory loggerFactory, IDeviceFactory deviceFactory)
        {
            DataLayer = dataLayer;
            Logger = logger;
            LoggerFactory = loggerFactory;
            DeviceFactory = deviceFactory;
        }

        public void AddDevice(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name parameter cannot be null or empty", "name");
            }

            DeviceModel deviceModel = new DeviceModel() { Name = name };
            DataLayer.AddUpdateDevices(deviceModel);

            CreateDevice(deviceModel);
        }

        private void CreateDevice(DeviceModel deviceModel)
        {
            IDevice device = DeviceFactory.CreateDemoDevice(deviceModel, LoggerFactory);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Thread deviceThread = new Thread(async () => await device.Run(cancellationTokenSource.Token));

            Devices.Add(deviceModel.Id ?? 0, new DeviceProcess(device, cancellationTokenSource, deviceThread));
            deviceThread.Start();
        }

        public IDevice GetDevice(int id)
        {
            throw new System.NotImplementedException();
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
