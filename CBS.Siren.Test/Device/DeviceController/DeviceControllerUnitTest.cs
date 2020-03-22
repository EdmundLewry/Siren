using CBS.Siren.Device;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CBS.Siren.Test.Device
{
    public class DeviceControllerUnitTest
    {
        private DeviceList GenerateTestDeviceList()
        {
            DeviceListEvent deviceListEvent = GenerateDeviceListEvent(DateTime.Now.AddMilliseconds(50), DateTime.Now.AddSeconds(1));
            DeviceList deviceList = new DeviceList(new List<DeviceListEvent>() { deviceListEvent });

            return deviceList;
        }

        private DeviceListEvent GenerateDeviceListEvent(DateTime startTime, DateTime endTime)
        {
            string eventData = $"{{\"timing\":{{\"startTime\":\"{startTime.ToString()}\",\"duration\":25,\"endTime\":\"{endTime.ToString()}\"}}}}";

            DeviceListEvent deviceListEvent = new DeviceListEvent(eventData);

            return deviceListEvent;
        }

        [Fact]
        [Trait("TestType","UnitTest")]
        public void WhenDeviceListSet_DeviceController_SetsCurrentEvent()
        {
            IDeviceController deviceController = new DeviceController();
            DeviceList generatedList = GenerateTestDeviceList();

            deviceController.ActiveDeviceList = generatedList;

            Assert.Equal(generatedList.Events[0], deviceController.CurrentEvent);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task WhenCurrentEventEnds_DeviceController_ChangesCurrentEvent()
        {
            IDeviceController deviceController = new DeviceController();
            DeviceList generatedList = GenerateTestDeviceList();
            generatedList.Events.Add(GenerateDeviceListEvent(DateTime.Now.AddSeconds(2), DateTime.Now.AddSeconds(3)));

            deviceController.ActiveDeviceList = generatedList;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(5000);
            Task deviceControllerTask = Task.Run(() => deviceController.Run(cancellationTokenSource.Token));

            deviceController.OnEventEnded += (sender, args) => {
                if(args.AffectedEvent.Id == generatedList.Events[0].Id)
                { 
                    cancellationTokenSource.Cancel();
                }
            };

            await deviceControllerTask;

            Assert.Equal(generatedList.Events[1], deviceController.CurrentEvent);

            if(cancellationTokenSource.Token.CanBeCanceled)
            { 
                cancellationTokenSource.Cancel();
            }
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task WhenEventStartTimeIsMet_DeviceController_EmitsEventStartEvent()
        {
            IDeviceController deviceController = new DeviceController();

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(5000);

            DeviceList deviceList = GenerateTestDeviceList();
            var evt = await Assert.RaisesAsync<DeviceEventChangedEventArgs>(
                h => deviceController.OnEventStarted += h,
                h => deviceController.OnEventStarted -= h,
                () => Task.Run(() => {
                            deviceController.ActiveDeviceList = deviceList;
                            deviceController.Run(cancellationTokenSource.Token);
                        })
            );

            DeviceEventChangedEventArgs expectedArgs = new DeviceEventChangedEventArgs(deviceList.Events[0]);

            Assert.NotNull(evt);
            Assert.Equal(deviceController, evt.Sender);
            Assert.Equal(expectedArgs, evt.Arguments);

            cancellationTokenSource.Cancel();
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task WhenEventEndTimeIsMet_DeviceController_EmitsEventEndEvent()
        {
            IDeviceController deviceController = new DeviceController();

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(5000);

            DeviceList deviceList = GenerateTestDeviceList();
            var evt = await Assert.RaisesAsync<DeviceEventChangedEventArgs>(
                h => deviceController.OnEventEnded += h,
                h => deviceController.OnEventEnded -= h,
                () => Task.Run(() => {
                    deviceController.ActiveDeviceList = deviceList;
                    deviceController.Run(cancellationTokenSource.Token);
                })
            );

            DeviceEventChangedEventArgs expectedArgs = new DeviceEventChangedEventArgs(deviceList.Events[0]);

            Assert.NotNull(evt);
            Assert.Equal(deviceController, evt.Sender);
            Assert.Equal(expectedArgs, evt.Arguments);

            cancellationTokenSource.Cancel();
        }
    }
}
