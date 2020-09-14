using CBS.Siren.Device;
using CBS.Siren.Time;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
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
            string eventData = $"{{\"timing\":{{\"startTime\":\"{startTime.ToTimecodeString()}\",\"duration\":\"00:00:25:00\",\"endTime\":\"{endTime.ToTimecodeString()}\"}}}}";

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

            deviceController.OnEventEnd += (sender, args) => {
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
                h => deviceController.OnEventStart += h,
                h => deviceController.OnEventStart -= h,
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
            deviceController.OnEventStart += eventHandler;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);

            DeviceList deviceList = GenerateTestDeviceList();
            
            deviceController.ActiveDeviceList = deviceList;
            await deviceController.Run(cancellationTokenSource.Token);

            DateTime expectedTime = deviceList.Events[0].StartTime;
            Assert.Equal(0, expectedTime.DifferenceInFrames(eventTime));

            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }

            deviceController.OnEventStart -= eventHandler;
        }


        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task WhenEventEndTimeIsMet_DeviceController_EmitsEventEndEvent()
        {
            IDeviceController deviceController = new DeviceController(new Mock<ILogger>().Object);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);

            DeviceList deviceList = GenerateTestDeviceList();
            var evt = await Assert.RaisesAsync<DeviceEventChangedEventArgs>(
                h => deviceController.OnEventEnd += h,
                h => deviceController.OnEventEnd -= h,
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
            deviceController.OnEventEnd += eventHandler;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);

            DeviceList deviceList = GenerateTestDeviceList();

            deviceController.ActiveDeviceList = deviceList;
            await deviceController.Run(cancellationTokenSource.Token);

            DateTime expectedTime = deviceList.Events[0].EndTime;
            Assert.Equal(0, expectedTime.DifferenceInFrames(eventTime));

            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }

            deviceController.OnEventEnd -= eventHandler;
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task DeviceController_EmitsDeviceListEvent_AtEndOfFinalEvent()
        {
            IDeviceController deviceController = new DeviceController(new Mock<ILogger>().Object);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);

            DeviceList deviceList = GenerateTestDeviceList();

            var evt = await Assert.RaisesAsync<EventArgs>(
                h => deviceController.OnDeviceListEnded += h,
                h => deviceController.OnDeviceListEnded -= h,
                () => Task.Run(async () =>
                {
                    deviceController.ActiveDeviceList = deviceList;
                    await deviceController.Run(cancellationTokenSource.Token);
                })
            );

            Assert.NotNull(evt);
            Assert.Equal(deviceController, evt.Sender);

            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }
        }

        //Could expose and test the internal functions and make the above integration tests - But choosing not to as this is a demo implementation

        [Fact]
        [Trait("TestType","UnitTest")]
        public void WhenDeviceListSet_DeviceController_SetsEventsStateToUnscheduled()
        {
            IDeviceController deviceController = new DeviceController(new Mock<ILogger>().Object);
            DeviceList generatedList = GenerateTestDeviceList();

            deviceController.ActiveDeviceList = generatedList;

            Assert.Equal(DeviceListEventState.Status.CUED, generatedList.Events[0].EventState.CurrentStatus);
        }
        
        [Fact]
        [Trait("TestType","UnitTest")]
        public async Task WhenEventStarts_DeviceController_SetsEventsStateToPlaying()
        {
            TaskCompletionSource<DeviceListEvent> taskCompletionSource = new TaskCompletionSource<DeviceListEvent>();
            EventHandler<DeviceEventChangedEventArgs> eventHandler = new EventHandler<DeviceEventChangedEventArgs>((sender, args) => taskCompletionSource.SetResult(args.AffectedEvent));
            IDeviceController deviceController = new DeviceController(new Mock<ILogger>().Object);
            deviceController.OnEventStart += eventHandler;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);

            DeviceList deviceList = GenerateTestDeviceList();

            deviceController.ActiveDeviceList = deviceList;
            _ = deviceController.Run(cancellationTokenSource.Token);

            await taskCompletionSource.Task;

            Assert.Equal(DeviceListEventState.Status.PLAYING, deviceList.Events[0].EventState.CurrentStatus);

            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }

            deviceController.OnEventStart -= eventHandler;
        }
        
        [Fact]
        [Trait("TestType","UnitTest")]
        public async Task WhenEventEnds_DeviceController_SetsEventsStateToPlayed()
        {
            TaskCompletionSource<DeviceListEvent> taskCompletionSource = new TaskCompletionSource<DeviceListEvent>();
            EventHandler<DeviceEventChangedEventArgs> eventHandler = new EventHandler<DeviceEventChangedEventArgs>((sender, args) => taskCompletionSource.SetResult(args.AffectedEvent));
            IDeviceController deviceController = new DeviceController(new Mock<ILogger>().Object);
            deviceController.OnEventEnd += eventHandler;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);

            DeviceList deviceList = GenerateTestDeviceList();

            deviceController.ActiveDeviceList = deviceList;
            _ = deviceController.Run(cancellationTokenSource.Token);

            await taskCompletionSource.Task;

            Assert.Equal(DeviceListEventState.Status.PLAYED, deviceList.Events[0].EventState.CurrentStatus);

            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }

            deviceController.OnEventEnd -= eventHandler;
        }
    }
}
