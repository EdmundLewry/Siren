﻿using CBS.Siren.Device;
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
        private readonly ITimeSourceProvider Clock;

        public DeviceControllerUnitTest()
        {
            Clock = new UtcTimeSourceProvider();
        }

        private DeviceList GenerateTestDeviceList(int count = 1)
        {
            List<DeviceListEvent> events = new List<DeviceListEvent>();
            for(int i=1; i<=count; ++i)
            {
                events.Add(GenerateDeviceListEvent(Clock.Now.AddMilliseconds(50*i), Clock.Now.AddSeconds(i)));
            }

            return new DeviceList(events);
        }

        private DeviceListEvent GenerateDeviceListEvent(DateTimeOffset startTime, DateTimeOffset endTime)
        {
            string eventData = $"{{\"timing\":{{\"startTime\":\"{startTime.ToTimecodeString()}\",\"duration\":\"00:00:25:00\",\"endTime\":\"{endTime.ToTimecodeString()}\"}}}}";

            return new DeviceListEvent(eventData);
        }

        private IDeviceController CreateDeviceController()
        {
            return new DeviceController(new Mock<ILogger>().Object, new Mock<IDeviceListEventStore>().Object) { Clock = Clock };
        }

        #region Device List Set
        [Fact]
        [Trait("TestType","UnitTest")]
        public void WhenDeviceListSet_DeviceController_SetsCurrentEvent()
        {
            IDeviceController deviceController = CreateDeviceController();
            DeviceList generatedList = GenerateTestDeviceList();

            deviceController.ActiveDeviceList = generatedList;

            Assert.Equal(generatedList.Events[0].Id, deviceController.CurrentEvent.Id);
        }
        
        [Fact]
        [Trait("TestType","UnitTest")]
        public void WhenDeviceListSet_WithExistingActiveListAndEventhasBeenAdded_AddsNewEvent()
        {
            IDeviceController deviceController = CreateDeviceController();
            DeviceList generatedList = GenerateTestDeviceList();

            deviceController.ActiveDeviceList = generatedList;
            
            DeviceList updateList = GenerateTestDeviceList(2);
            updateList.Events[0].Id = generatedList.Events[0].Id;
            deviceController.ActiveDeviceList = updateList;

            Assert.Equal(generatedList.Events[0].Id, deviceController.CurrentEvent.Id);
            Assert.Equal(2, deviceController.ActiveDeviceList.Events.Count);
            Assert.Equal(updateList.Events[0].Id, deviceController.ActiveDeviceList.Events[0].Id);
            Assert.All(deviceController.ActiveDeviceList.Events, (listEvent) => Assert.Equal(DeviceListEventStatus.CUED, listEvent.EventState.CurrentStatus));
        }
        
        [Fact]
        [Trait("TestType","UnitTest")]
        public void WhenDeviceListSet_WithExistingActiveListAndEventhasBeenAdded_SetsNewEventToCued()
        {
            IDeviceController deviceController = CreateDeviceController();
            DeviceList generatedList = GenerateTestDeviceList();

            deviceController.ActiveDeviceList = generatedList;
            
            DeviceList updateList = GenerateTestDeviceList(2);
            updateList.Events[0].Id = generatedList.Events[0].Id;
            deviceController.ActiveDeviceList = updateList;

            Assert.All(deviceController.ActiveDeviceList.Events, (listEvent) => Assert.Equal(DeviceListEventStatus.CUED, listEvent.EventState.CurrentStatus));
        }
        
        [Fact]
        [Trait("TestType","UnitTest")]
        public void WhenDeviceListSet_WithExistingActiveListAndEventhasBeenReplaced_ReplacesEvent()
        {
            IDeviceController deviceController = CreateDeviceController();
            DeviceList generatedList = GenerateTestDeviceList(2);

            deviceController.ActiveDeviceList = generatedList;

            DeviceList updateList = new DeviceList(new List<DeviceListEvent>() { generatedList.Events[0] });
            updateList.Events.Add(GenerateDeviceListEvent(Clock.Now.AddSeconds(2), Clock.Now.AddSeconds(3)));
            deviceController.ActiveDeviceList = updateList;

            Assert.Equal(generatedList.Events[0].Id, deviceController.CurrentEvent.Id);
            Assert.Equal(2, deviceController.ActiveDeviceList.Events.Count);
            Assert.Equal(generatedList.Events[0].Id, deviceController.ActiveDeviceList.Events[0].Id);
            Assert.Equal(updateList.Events[1].Id, deviceController.ActiveDeviceList.Events[1].Id);
        }
        
        [Fact]
        [Trait("TestType","UnitTest")]
        public void WhenDeviceListSet_WithExistingActiveListAndEventAddedAboveKnownEvent_ReplacesFromThatEvent()
        {
            IDeviceController deviceController = CreateDeviceController();
            DeviceList generatedList = GenerateTestDeviceList(2);

            deviceController.ActiveDeviceList = generatedList;

            DeviceList updateList = new DeviceList(new List<DeviceListEvent>() { generatedList.Events[0] });
            updateList.Events.Add(GenerateDeviceListEvent(Clock.Now.AddSeconds(2), Clock.Now.AddSeconds(3)));
            updateList.Events.Add(generatedList.Events[1]);

            deviceController.ActiveDeviceList = updateList;

            Assert.Equal(generatedList.Events[0].Id, deviceController.CurrentEvent.Id);
            Assert.Equal(3, deviceController.ActiveDeviceList.Events.Count);
            Assert.Equal(generatedList.Events[0].Id, deviceController.ActiveDeviceList.Events[0].Id);
            Assert.Equal(updateList.Events[1].Id, deviceController.ActiveDeviceList.Events[1].Id);
            Assert.Equal(generatedList.Events[1].Id, deviceController.ActiveDeviceList.Events[2].Id);
        }
        
        [Fact]
        [Trait("TestType","UnitTest")]
        public void WhenDeviceListSet_WithExistingActiveListAndFinalEventDeleted_RemovesFinalEvent()
        {
            IDeviceController deviceController = CreateDeviceController();
            DeviceList generatedList = GenerateTestDeviceList(2);

            deviceController.ActiveDeviceList = generatedList;

            DeviceList updateList = new DeviceList(new List<DeviceListEvent>() { generatedList.Events[0] });
            deviceController.ActiveDeviceList = updateList;

            Assert.Equal(generatedList.Events[0].Id, deviceController.CurrentEvent.Id);
            Assert.Single(deviceController.ActiveDeviceList.Events);
            Assert.Equal(updateList.Events[0].Id, deviceController.ActiveDeviceList.Events[0].Id);
        }
        
        [Fact]
        [Trait("TestType","UnitTest")]
        public void WhenDeviceListSet_WithExistingActiveListAndMiddleEventDeleted_RemovesMiddleEventFromActiveList()
        {
            IDeviceController deviceController = CreateDeviceController();
            DeviceList generatedList = GenerateTestDeviceList(3);

            deviceController.ActiveDeviceList = generatedList;

            DeviceList updateList = new DeviceList(new List<DeviceListEvent>() { generatedList.Events[0], generatedList.Events[2] });
            deviceController.ActiveDeviceList = updateList;

            Assert.Equal(generatedList.Events[0].Id, deviceController.CurrentEvent.Id);
            Assert.Equal(2, deviceController.ActiveDeviceList.Events.Count);
            Assert.Equal(generatedList.Events[0].Id, deviceController.ActiveDeviceList.Events[0].Id);
            Assert.Equal(generatedList.Events[2].Id, deviceController.ActiveDeviceList.Events[1].Id);
        }

        [Fact]
        [Trait("TestType","UnitTest")]
        public void WhenDeviceListSet_WithExistingActiveListAndEventTimingDataHasChanged_ReplacesEvent()
        {
            IDeviceController deviceController = CreateDeviceController();
            DeviceList generatedList = GenerateTestDeviceList(2);

            deviceController.ActiveDeviceList = generatedList;

            DeviceList updateList = new DeviceList(new List<DeviceListEvent>() { generatedList.Events[0] });
            DeviceListEvent deviceListEvent = GenerateDeviceListEvent(Clock.Now.AddSeconds(2), Clock.Now.AddSeconds(20));
            deviceListEvent.Id = generatedList.Events[1].Id;
            updateList.Events.Add(deviceListEvent);
            deviceController.ActiveDeviceList = updateList;

            Assert.Equal(generatedList.Events[0].Id, deviceController.CurrentEvent.Id);
            Assert.Equal(2, deviceController.ActiveDeviceList.Events.Count);
            Assert.Equal(generatedList.Events[0].Id, deviceController.ActiveDeviceList.Events[0].Id);
            Assert.Equal(updateList.Events[1].Id, deviceController.ActiveDeviceList.Events[1].Id);
            Assert.Equal(updateList.Events[1].StartTime, deviceController.ActiveDeviceList.Events[1].StartTime);
            Assert.Equal(updateList.Events[1].EndTime, deviceController.ActiveDeviceList.Events[1].EndTime);
        }
        
        [Fact]
        [Trait("TestType","UnitTest")]
        public void WhenDeviceListSet_WithExistingActiveListAndActiveEventTimingDataHasChanged_UpdatesEventEndTime()
        {
            IDeviceController deviceController = CreateDeviceController();
            DeviceList generatedList = GenerateTestDeviceList(2);
            DateTimeOffset originalStartTime = generatedList.Events[0].StartTime;

            deviceController.ActiveDeviceList = generatedList;

            DateTimeOffset newEndTime = Clock.Now.AddSeconds(20);
            DeviceListEvent deviceListEvent = GenerateDeviceListEvent(originalStartTime, newEndTime);
            deviceListEvent.Id = generatedList.Events[0].Id;
            DeviceList updateList = new DeviceList(new List<DeviceListEvent>() { deviceListEvent, generatedList.Events[1] });
            deviceController.ActiveDeviceList = updateList;

            Assert.Equal(generatedList.Events[0].Id, deviceController.CurrentEvent.Id);
            Assert.Equal(2, deviceController.ActiveDeviceList.Events.Count);
            Assert.Equal(updateList.Events[0].Id, deviceController.ActiveDeviceList.Events[0].Id);
            Assert.Equal(updateList.Events[1].Id, deviceController.ActiveDeviceList.Events[1].Id);
            Assert.Equal(originalStartTime, deviceController.ActiveDeviceList.Events[0].StartTime);
            //When we construct the event data while generating the DeviceListEvent we set times to Timecode Strings which
            //loses the fideleity of the milliseconds by rounding to the nearest frame. So compare timecode strings here
            Assert.Equal(newEndTime.ToTimecodeString(), deviceController.ActiveDeviceList.Events[0].EndTime.ToTimecodeString());
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void WhenDeviceListSet_DeviceController_SetsEventsStateToCued()
        {
            IDeviceController deviceController = CreateDeviceController();
            DeviceList generatedList = GenerateTestDeviceList();

            deviceController.ActiveDeviceList = generatedList;

            Assert.Equal(DeviceListEventStatus.CUED, deviceController.ActiveDeviceList.Events[0].EventState.CurrentStatus);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void WhenDeviceListSet_WithExistingActiveListAndIncomingListIsNull_ClearsList()
        {
            IDeviceController deviceController = CreateDeviceController();
            DeviceList generatedList = GenerateTestDeviceList(2);

            deviceController.ActiveDeviceList = generatedList;

            deviceController.ActiveDeviceList = null;

            Assert.Null(deviceController.ActiveDeviceList);
        }

        #endregion

        #region Event Start
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task WhenEventStartTimeIsMet_DeviceController_EmitsEventStartEvent()
        {
            IDeviceController deviceController = CreateDeviceController();

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
        public async Task DeviceController_WhenListIsSetWithNewInitialEvent_EmitsEventStartEvent()
        {
            IDeviceController deviceController = CreateDeviceController();

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);

            DeviceList deviceList = GenerateTestDeviceList();
            DeviceList replacementList = GenerateTestDeviceList();

            int count = 0;
            EventHandler<DeviceEventChangedEventArgs> eventHandler = new EventHandler<DeviceEventChangedEventArgs>(
                (sender, args) => {
                    count++;
                    if (count == 1)
                    {
                        deviceController.ActiveDeviceList = replacementList;
                    }
                }
            );

            deviceController.OnEventStarted += eventHandler;
            deviceController.ActiveDeviceList = deviceList;
            await deviceController.Run(cancellationTokenSource.Token);

            Assert.Equal(2, count);

            deviceController.OnEventStarted -= eventHandler;
            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task DeviceController_WhenListIsSetWithSameInitialEvent_DoesNotEmitStartAgain()
        {
            IDeviceController deviceController = CreateDeviceController();

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(5000);

            DeviceList deviceList = new DeviceList(new List<DeviceListEvent>(){
                GenerateDeviceListEvent(Clock.Now.AddMilliseconds(50), Clock.Now.AddMinutes(2))
            });
            DeviceList replacementList = new DeviceList(deviceList);
            replacementList.Events.Add(GenerateDeviceListEvent(Clock.Now.AddMinutes(2), Clock.Now.AddMinutes(3)));

            int count = 0;
            EventHandler<DeviceEventChangedEventArgs> eventHandler = new EventHandler<DeviceEventChangedEventArgs>(
                (sender, args) => {
                    count++;
                    if (count == 1)
                    {
                        deviceController.ActiveDeviceList = replacementList;
                    }
                }
            );

            deviceController.OnEventStarted += eventHandler;
            deviceController.ActiveDeviceList = deviceList;
            await deviceController.Run(cancellationTokenSource.Token);

            Assert.Equal(1, count);

            deviceController.OnEventStarted -= eventHandler;
            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task WhenEventStartTimeIsMet_DeviceController_CallsIntoDataStore()
        {
            var deviceListEventStore = new Mock<IDeviceListEventStore>();
            IDeviceController deviceController = new DeviceController(new Mock<ILogger>().Object, deviceListEventStore.Object);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);

            DeviceList deviceList = GenerateTestDeviceList();
            await Assert.RaisesAsync<DeviceEventChangedEventArgs>(
                h => deviceController.OnEventStarted += h,
                h => deviceController.OnEventStarted -= h,
                () => Task.Run(async () => {
                            deviceController.ActiveDeviceList = deviceList;
                            await deviceController.Run(cancellationTokenSource.Token);
                        })
            );

            deviceListEventStore.Verify(mock => mock.UpdateDeviceListEvent(It.IsAny<DeviceListEvent>()), Times.AtLeastOnce);

            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task DeviceController_EmitsEventStartEvent_AtStartFrameOfEvent()
        {
            DateTimeOffset eventTime = Clock.Now;
            EventHandler<DeviceEventChangedEventArgs> eventHandler = new EventHandler<DeviceEventChangedEventArgs>((sender, args) => eventTime = Clock.Now);
            IDeviceController deviceController = CreateDeviceController();
            deviceController.OnEventStarted += eventHandler;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);

            DeviceList deviceList = GenerateTestDeviceList();
            
            deviceController.ActiveDeviceList = deviceList;
            await deviceController.Run(cancellationTokenSource.Token);

            DateTimeOffset expectedTime = deviceList.Events[0].StartTime;
            Assert.Equal(0, expectedTime.DifferenceInFrames(eventTime));

            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }

            deviceController.OnEventStarted -= eventHandler;
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task WhenEventStarts_DeviceController_SetsEventsStateToPlaying()
        {
            TaskCompletionSource<DeviceListEvent> taskCompletionSource = new TaskCompletionSource<DeviceListEvent>();
            EventHandler<DeviceEventChangedEventArgs> eventHandler = new EventHandler<DeviceEventChangedEventArgs>((sender, args) => taskCompletionSource.SetResult(args.AffectedEvent));
            IDeviceController deviceController = CreateDeviceController();
            deviceController.OnEventStarted += eventHandler;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);

            DeviceList deviceList = GenerateTestDeviceList();

            deviceController.ActiveDeviceList = deviceList;
            Task controllerRun = deviceController.Run(cancellationTokenSource.Token);

            await Task.WhenAny(taskCompletionSource.Task, controllerRun);

            Assert.Equal(DeviceListEventStatus.PLAYING, deviceController.ActiveDeviceList.Events[0].EventState.CurrentStatus);

            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }

            deviceController.OnEventStarted -= eventHandler;
        }
        #endregion

        #region Event End
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task WhenCurrentEventEnds_DeviceController_ChangesCurrentEvent()
        {
            IDeviceController deviceController = CreateDeviceController();
            DeviceList generatedList = GenerateTestDeviceList();
            generatedList.Events.Add(GenerateDeviceListEvent(Clock.Now.AddSeconds(2), Clock.Now.AddSeconds(3)));

            deviceController.ActiveDeviceList = generatedList;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);
            Task deviceControllerTask = Task.Run(() => deviceController.Run(cancellationTokenSource.Token));

            deviceController.OnEventEnded += (sender, args) => {
                if (args.AffectedEvent.Id == generatedList.Events[0].Id)
                {
                    cancellationTokenSource.Cancel();
                }
            };

            await deviceControllerTask;

            Assert.Equal(generatedList.Events[1].Id, deviceController.CurrentEvent.Id);

            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task WhenEventEndTimeIsMet_DeviceController_EmitsEventEndEvent()
        {
            IDeviceController deviceController = CreateDeviceController();

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
        public async Task WhenEventEndTimeIsMet_DeviceController_CallsIntoDataStore()
        {
            var deviceListEventStore = new Mock<IDeviceListEventStore>();
            IDeviceController deviceController = new DeviceController(new Mock<ILogger>().Object, deviceListEventStore.Object);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);

            DeviceList deviceList = GenerateTestDeviceList();
            _ = await Assert.RaisesAsync<DeviceEventChangedEventArgs>(
                h => deviceController.OnEventEnded += h,
                h => deviceController.OnEventEnded -= h,
                () => Task.Run(async () => {
                    deviceController.ActiveDeviceList = deviceList;
                    await deviceController.Run(cancellationTokenSource.Token);
                })
            );

            deviceListEventStore.Verify(mock => mock.UpdateDeviceListEvent(It.IsAny<DeviceListEvent>()), Times.AtLeastOnce);

            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task DeviceController_EmitsEventEndEvent_AtEndFrameOfEvent()
        {
            DateTimeOffset eventTime = Clock.Now;
            EventHandler<DeviceEventChangedEventArgs> eventHandler = new EventHandler<DeviceEventChangedEventArgs>((sender, args) => eventTime = Clock.Now);
            IDeviceController deviceController = CreateDeviceController();
            deviceController.OnEventEnded += eventHandler;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);

            DeviceList deviceList = GenerateTestDeviceList();

            deviceController.ActiveDeviceList = deviceList;
            await deviceController.Run(cancellationTokenSource.Token);

            DateTimeOffset expectedTime = deviceList.Events[0].EndTime;
            Assert.Equal(0, expectedTime.DifferenceInFrames(eventTime));

            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }

            deviceController.OnEventEnded -= eventHandler;
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task DeviceController_EmitsDeviceListEvent_AtEndOfFinalEvent()
        {
            IDeviceController deviceController = CreateDeviceController();

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

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task WhenEventEnds_DeviceController_SetsEventsStateToPlayed()
        {
            TaskCompletionSource<DeviceListEvent> taskCompletionSource = new TaskCompletionSource<DeviceListEvent>();
            EventHandler<DeviceEventChangedEventArgs> eventHandler = new EventHandler<DeviceEventChangedEventArgs>((sender, args) => taskCompletionSource.SetResult(args.AffectedEvent));
            IDeviceController deviceController = CreateDeviceController();
            deviceController.OnEventEnded += eventHandler;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);

            DeviceList deviceList = GenerateTestDeviceList();

            deviceController.ActiveDeviceList = deviceList;
            Task controllerRun = deviceController.Run(cancellationTokenSource.Token);

            await Task.WhenAny(taskCompletionSource.Task, controllerRun);

            Assert.True(taskCompletionSource.Task.IsCompleted);

            Assert.Equal(DeviceListEventStatus.PLAYED, taskCompletionSource.Task.Result.EventState.CurrentStatus);

            if (cancellationTokenSource.Token.CanBeCanceled)
            {
                cancellationTokenSource.Cancel();
            }

            deviceController.OnEventEnded -= eventHandler;
        }
        #endregion
    }
}
