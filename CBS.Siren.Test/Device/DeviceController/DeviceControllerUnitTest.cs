﻿using CBS.Siren.Device;
using CBS.Siren.Time;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CBS.Siren.Test.Device
{
    public class DeviceControllerUnitTest
    {
        private const int TIMEOUT = 2000;

        private DeviceList GenerateTestDeviceList()
        {
            DeviceListEvent deviceListEvent = GenerateDeviceListEvent(DateTime.Now.AddMilliseconds(50), DateTime.Now.AddSeconds(1));
            DeviceList deviceList = new DeviceList(new List<DeviceListEvent>() { deviceListEvent });

            return deviceList;
        }

        private DeviceListEvent GenerateDeviceListEvent(DateTime startTime, DateTime endTime)
        {
            string eventData = $"{{\"timing\":{{\"startTime\":\"{startTime.ToString("o")}\",\"duration\":25,\"endTime\":\"{endTime.ToString("o")}\"}}}}";

            DeviceListEvent deviceListEvent = new DeviceListEvent(eventData);

            return deviceListEvent;
        }

        [Fact]
        [Trait("TestType","UnitTest")]
        public void WhenDeviceListSet_DeviceController_SetsCurrentEvent()
        {
            IDeviceController deviceController = new DeviceController(new Mock<ILogger>().Object);
            DeviceList generatedList = GenerateTestDeviceList();

            deviceController.ActiveDeviceList = generatedList;

            Assert.Equal(generatedList.Events[0], deviceController.CurrentEvent);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task WhenCurrentEventEnds_DeviceController_ChangesCurrentEvent()
        {
            IDeviceController deviceController = new DeviceController(new Mock<ILogger>().Object);
            DeviceList generatedList = GenerateTestDeviceList();
            generatedList.Events.Add(GenerateDeviceListEvent(DateTime.Now.AddSeconds(2), DateTime.Now.AddSeconds(3)));

            deviceController.ActiveDeviceList = generatedList;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);
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
            IDeviceController deviceController = new DeviceController(new Mock<ILogger>().Object);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);

            DeviceList deviceList = GenerateTestDeviceList();
            var evt = await Assert.RaisesAsync<DeviceEventChangedEventArgs>(
                h => deviceController.OnEventStarted += h,
                h => deviceController.OnEventStarted -= h,
                () => Task.Run(async () => {
                            deviceController.ActiveDeviceList = deviceList;
                            await deviceController.Run(cancellationTokenSource.Token);
                        })
            );

            DeviceEventChangedEventArgs expectedArgs = new DeviceEventChangedEventArgs(deviceList.Events[0]);

            Assert.NotNull(evt);
            Assert.Equal(deviceController, evt.Sender);
            Assert.Equal(expectedArgs.AffectedEvent.Id, evt.Arguments.AffectedEvent.Id);

            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task DeviceController_EmitsEventStartEvent_AtStartFrameOfEvent()
        {
            DateTime eventTime = DateTime.Now;
            EventHandler<DeviceEventChangedEventArgs> eventHandler = new EventHandler<DeviceEventChangedEventArgs>((sender, args) => eventTime = DateTime.Now);
            IDeviceController deviceController = new DeviceController(new Mock<ILogger>().Object);
            deviceController.OnEventStarted += eventHandler;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);

            DeviceList deviceList = GenerateTestDeviceList();
            
            deviceController.ActiveDeviceList = deviceList;
            await deviceController.Run(cancellationTokenSource.Token);

            //TODO:2 Move this to within the DeviceListEvent
            JsonElement startTimeElement = JsonDocument.Parse(deviceList.Events[0].EventData).RootElement.GetProperty("timing").GetProperty("startTime");
            DateTime expectedTime = DateTime.Parse(startTimeElement.GetString());
            Assert.Equal(0, expectedTime.DifferenceInFrames(eventTime));

            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }

            deviceController.OnEventStarted -= eventHandler;
        }


        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task WhenEventEndTimeIsMet_DeviceController_EmitsEventEndEvent()
        {
            IDeviceController deviceController = new DeviceController(new Mock<ILogger>().Object);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);

            DeviceList deviceList = GenerateTestDeviceList();
            var evt = await Assert.RaisesAsync<DeviceEventChangedEventArgs>(
                h => deviceController.OnEventEnded += h,
                h => deviceController.OnEventEnded -= h,
                () => Task.Run(async () => {
                    deviceController.ActiveDeviceList = deviceList;
                    await deviceController.Run(cancellationTokenSource.Token);
                })
            );

            DeviceEventChangedEventArgs expectedArgs = new DeviceEventChangedEventArgs(deviceList.Events[0]);

            Assert.NotNull(evt);
            Assert.Equal(deviceController, evt.Sender);
            Assert.Equal(expectedArgs.AffectedEvent.Id, evt.Arguments.AffectedEvent.Id);

            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task DeviceController_EmitsEventEndEvent_AtEndFrameOfEvent()
        {
            DateTime eventTime = DateTime.Now;
            EventHandler<DeviceEventChangedEventArgs> eventHandler = new EventHandler<DeviceEventChangedEventArgs>((sender, args) => eventTime = DateTime.Now);
            IDeviceController deviceController = new DeviceController(new Mock<ILogger>().Object);
            deviceController.OnEventEnded += eventHandler;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);

            DeviceList deviceList = GenerateTestDeviceList();

            deviceController.ActiveDeviceList = deviceList;
            await deviceController.Run(cancellationTokenSource.Token);

            //TODO:2 Move this to within the DeviceListEvent
            JsonElement endTimeElement = JsonDocument.Parse(deviceList.Events[0].EventData).RootElement.GetProperty("timing").GetProperty("endTime");
            DateTime expectedTime = DateTime.Parse(endTimeElement.GetString());
            Assert.Equal(0, expectedTime.DifferenceInFrames(eventTime));

            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }

            deviceController.OnEventEnded -= eventHandler;
        }

        //Could I expose and test the internal functions and make the above integration tests - But choosing not to as this is a demo implementation
    }
}
