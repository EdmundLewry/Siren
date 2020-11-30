using CBS.Siren.Device;
using CBS.Siren.Time;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace CBS.Siren.Test
{
    public class TransmissionListServiceUnitTest
    {
        #region Test Setup
        private TransmissionListEvent GenerateTestTransmissionListEvent(params IDevice[] devices)
        {
            List<IEventFeature> eventFeatures = new List<IEventFeature>();

            foreach (IDevice device in devices)
            {
                var eventFeature = new Mock<IEventFeature>();
                eventFeature.Setup(mock => mock.Device).Returns(device);
                eventFeature.SetupProperty(mock => mock.DeviceListEventId);
                eventFeatures.Add(eventFeature.Object);
            }

            return new TransmissionListEvent(new Mock<IEventTimingStrategy>().Object, eventFeatures);
        }

        private Mock<IDeviceListEventStore> CreateMockDeviceListEventStore(params DeviceListEvent[] deviceEvents)
        {
            var mockDeviceEventStore = new Mock<IDeviceListEventStore>();
            foreach (DeviceListEvent deviceListEvent in deviceEvents)
            {
                mockDeviceEventStore.Setup(mock => mock.GetEventById(deviceListEvent.Id)).Returns(deviceListEvent);
            }

            return mockDeviceEventStore;
        }
        #endregion

        #region Play
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenPlayCalled_DeliversEventsToDevices()
        {
            var mockDevice = new Mock<IDevice>();

            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent(""), new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            List<TransmissionListEvent> events = new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, new List<IEventFeature>()){ Id = 1 }
            };
            TransmissionList transmissionList = new TransmissionList(events);
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.PlayTransmissionList();

            mockDevice.VerifySet(mock => mock.ActiveList = It.IsAny<DeviceList>());
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenPlayCalledWithNoCurrentEvent_SchedulesListFromStart()
        {
            var mockDevice = new Mock<IDevice>();

            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent(""), new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            List<TransmissionListEvent> events = new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, new List<IEventFeature>()){ Id = 1 }
            };
            TransmissionList transmissionList = new TransmissionList(events);
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.PlayTransmissionList();

            mockScheduler.Verify(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), 0));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenPlayCalledCurrentEventSet_SchedulesListFromEventIndex()
        {
            var mockDevice = new Mock<IDevice>();

            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent(""), new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            List<TransmissionListEvent> events = new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, new List<IEventFeature>()){ Id = 1 },
                new TransmissionListEvent(null, new List<IEventFeature>()){ Id = 3 },
                new TransmissionListEvent(null, new List<IEventFeature>()){ Id = 2 }
            };
            TransmissionList transmissionList = new TransmissionList(events) { CurrentEventId = 3 };
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.PlayTransmissionList();

            mockScheduler.Verify(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), 1));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenPlayCalledCurrentEventSetAndIsStopped_SchedulesListFromNextEventIndex()
        {
            var mockDevice = new Mock<IDevice>();

            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent(""), new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            List<TransmissionListEvent> events = new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, new List<IEventFeature>()){ Id = 1 },
                new TransmissionListEvent(null, new List<IEventFeature>()){ Id = 3, EventState = new TransmissionListEventState(){ CurrentStatus = TransmissionListEventState.Status.PLAYED } },
                new TransmissionListEvent(null, new List<IEventFeature>()){ Id = 2 }
            };
            TransmissionList transmissionList = new TransmissionList(events) { CurrentEventId = 3 };
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.PlayTransmissionList();

            mockScheduler.Verify(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), 2));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenPlayCalledOnPlayingList_DoesNothing()
        {
            var mockScheduler = new Mock<IScheduler>();

            List<TransmissionListEvent> events = new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, new List<IEventFeature>()){ Id = 1 },
                new TransmissionListEvent(null, new List<IEventFeature>()){ Id = 3 }
            };
            TransmissionList transmissionList = new TransmissionList(events) 
            { 
                State = TransmissionListState.Playing,
                CurrentEventId = 3 
            };
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.PlayTransmissionList();

            mockScheduler.Verify(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>()), Times.Never);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenPlayCalledOnEndOfList_DoesNothing()
        {
            var mockScheduler = new Mock<IScheduler>();

            List<TransmissionListEvent> events = new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, new List<IEventFeature>()){ Id = 1 },
                new TransmissionListEvent(null, new List<IEventFeature>()){ Id = 2, EventState = new TransmissionListEventState(){ CurrentStatus = TransmissionListEventState.Status.PLAYED } }
            };
            TransmissionList transmissionList = new TransmissionList(events) { CurrentEventId = 2 };
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.PlayTransmissionList();

            mockScheduler.Verify(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenPlayCalled_SetsListStateToPlaying()
        {
            var mockDevice = new Mock<IDevice>();

            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent(""), new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            List<TransmissionListEvent> events = new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, new List<IEventFeature>()){ Id = 1 }
            };
            TransmissionList transmissionList = new TransmissionList(events);
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.PlayTransmissionList();

            Assert.Equal(TransmissionListState.Playing, transmissionList.State);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenPlayCalledButNoDeviceListsGenerated_ListStateIsUnchanged()
        {
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(new Dictionary<IDevice, DeviceList>());

            List<TransmissionListEvent> events = new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, new List<IEventFeature>()){ Id = 1 }
            };
            TransmissionList transmissionList = new TransmissionList(events);
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.PlayTransmissionList();

            Assert.Equal(TransmissionListState.Stopped, transmissionList.State);
        }
        #endregion

        #region Stop
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenStopCalledOnPlayingList_DeliversNullListToDevices()
        {
            var mockDevice = new Mock<IDevice>();
            DeviceList existingList = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent("") });
            mockDevice.SetupProperty(mock => mock.ActiveList, existingList);

            var mockScheduler = new Mock<IScheduler>();

            Mock<IEventFeature> mockFeature = new Mock<IEventFeature>();
            mockFeature.Setup(mock => mock.Device).Returns(mockDevice.Object);
            TransmissionListEvent transmissionListEvent = new TransmissionListEvent(null, new List<IEventFeature>() { mockFeature.Object });
            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { transmissionListEvent }, null)
            {
                State = TransmissionListState.Playing
            };
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.StopTransmissionList();

            Assert.Null(mockDevice.Object.ActiveList);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenStopCalled_ClearsFeatureDeviceListEvents()
        {
            var mockDevice = new Mock<IDevice>();
            mockDevice.SetupProperty(mock => mock.ActiveList, null);

            var mockScheduler = new Mock<IScheduler>();

            Mock<IEventFeature> mockFeature = new Mock<IEventFeature>();
            mockFeature.Setup(mock => mock.Device).Returns(mockDevice.Object);
            mockFeature.SetupProperty(mock => mock.DeviceListEventId, 20);
            TransmissionListEvent transmissionListEvent = new TransmissionListEvent(null, new List<IEventFeature>() { mockFeature.Object });
            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { transmissionListEvent }, null)
            {
                State = TransmissionListState.Playing
            };
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.StopTransmissionList();

            Assert.Null(mockFeature.Object.DeviceListEventId);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenStopCalled_SetsListStateToStopped()
        {
            var mockDevice = new Mock<IDevice>();

            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent(""), new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            List<TransmissionListEvent> events = new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, new List<IEventFeature>()){ Id = 1 }
            };
            TransmissionList transmissionList = new TransmissionList(events);
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.PlayTransmissionList();
            serviceUnderTest.StopTransmissionList();

            Assert.Equal(TransmissionListState.Stopped, transmissionList.State);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenStopCalledOnStoppedList_ListStateIsUnchanged()
        {
            var mockDevice = new Mock<IDevice>();
            var mockScheduler = new Mock<IScheduler>();

            Mock<IEventFeature> mockFeature = new Mock<IEventFeature>();
            mockFeature.Setup(mock => mock.Device).Returns(mockDevice.Object);
            TransmissionListEvent transmissionListEvent = new TransmissionListEvent(null, new List<IEventFeature>() { mockFeature.Object });
            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { transmissionListEvent }, null);
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.StopTransmissionList();

            Assert.Equal(TransmissionListState.Stopped, transmissionList.State);
            mockDevice.VerifySet(mock => mock.ActiveList = It.IsAny<DeviceList>(), Times.Never);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenStopCalledCurrentEventSet_ResetsTheCurrentEvent()
        {
            var mockDevice = new Mock<IDevice>();

            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent(""), new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            List<TransmissionListEvent> events = new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, new List<IEventFeature>())
                { 
                    Id = 1, ActualStartTime = DateTimeOffset.Parse("12/02/2020 12:00:00"), 
                    EventState = new TransmissionListEventState(){ CurrentStatus = TransmissionListEventState.Status.PLAYED} 
                },
                new TransmissionListEvent(null, new List<IEventFeature>())
                { 
                    Id = 3, ActualStartTime = DateTimeOffset.Parse("12/02/2020 14:00:00"), 
                    EventState = new TransmissionListEventState(){ CurrentStatus = TransmissionListEventState.Status.PLAYING}
                },
                new TransmissionListEvent(null, new List<IEventFeature>()){ Id = 2 }
            };
            TransmissionList transmissionList = new TransmissionList(events, null) { CurrentEventId = 3 };
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.PlayTransmissionList();
            serviceUnderTest.StopTransmissionList();

            Assert.NotNull(transmissionList.Events[0].ActualStartTime);
            Assert.Equal(TransmissionListEventState.Status.PLAYED, transmissionList.Events[0].EventState.CurrentStatus);
            Assert.Null(transmissionList.Events[1].ActualStartTime);
            Assert.Equal(TransmissionListEventState.Status.SCHEDULED, transmissionList.Events[1].EventState.CurrentStatus);
        }
        #endregion

        #region Next
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void NextTransmissionList_WhileListIsPlaying_UpdatesActualEndOfCurrentEventToNow()
        {
            var mockDevice = new Mock<IDevice>();
            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(mockDevice.Object);
            TransmissionListEvent event2 = GenerateTestTransmissionListEvent(mockDevice.Object);
            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1, event2 }) { 
                State = TransmissionListState.Playing,
                CurrentEventId = event1.Id
            };

            DateTimeOffset expectedTime = DateTimeOffset.Parse("01/01/2020 12:00:00");
            Mock<ITimeSourceProvider> mockClock = new Mock<ITimeSourceProvider>();
            mockClock.Setup(mock => mock.Now).Returns(expectedTime);
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList,
                Clock = mockClock.Object
            };

            serviceUnderTest.NextTransmissionList();

            Assert.NotNull(transmissionList.Events[0].ActualEndTime);
            Assert.Equal(expectedTime, transmissionList.Events[0].ActualEndTime);
            Assert.Equal(TransmissionListEventState.Status.PLAYED, transmissionList.Events[0].EventState.CurrentStatus);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void NextTransmissionList_WhileCurrentEventIsNotLast_SetsNextEventAsCurrentEvent()
        {
            var mockDevice = new Mock<IDevice>();
            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(mockDevice.Object);
            TransmissionListEvent event2 = GenerateTestTransmissionListEvent(mockDevice.Object);
            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1, event2 })
            {
                State = TransmissionListState.Playing,
                CurrentEventId = event1.Id
            };

            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.NextTransmissionList();

            Assert.Equal(event2.Id, transmissionList.CurrentEventId);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void NextTransmissionList_WhenCurrentEventIsLastEvent_DoesNotChangeCurrentEvent()
        {
            var mockDevice = new Mock<IDevice>();
            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(mockDevice.Object);
            TransmissionListEvent event2 = GenerateTestTransmissionListEvent(mockDevice.Object);
            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1, event2 })
            {
                State = TransmissionListState.Playing,
                CurrentEventId = event2.Id
            };

            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.NextTransmissionList();

            Assert.Equal(event2.Id, transmissionList.CurrentEventId);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void NextTransmissionList_WhenCurrentEventIsLastEventAndListIsPlaying_StopsTheList()
        {
            var mockDevice = new Mock<IDevice>();
            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(mockDevice.Object);
            event1.Id = 1;
            TransmissionListEvent event2 = GenerateTestTransmissionListEvent(mockDevice.Object);
            event2.Id = 2;
            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1, event2 })
            {
                State = TransmissionListState.Playing,
                CurrentEventId = event2.Id
            };

            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.NextTransmissionList();

            Assert.Equal(TransmissionListState.Stopped, transmissionList.State);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void NextTransmissionList_WhenListIsEmpty_DoesNothing()
        {
            var mockScheduler = new Mock<IScheduler>();

            TransmissionList transmissionList = new TransmissionList();

            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.NextTransmissionList();

            Assert.Equal(TransmissionListState.Stopped, transmissionList.State);
            mockScheduler.Verify(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>()), Times.Never);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void NextTransmissionList_WhileListIsPlaying_SchedulesTheListWithTheCurrentEventIndex()
        {
            var mockDevice = new Mock<IDevice>();
            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(mockDevice.Object);
            TransmissionListEvent event2 = GenerateTestTransmissionListEvent(mockDevice.Object);
            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1, event2 })
            {
                State = TransmissionListState.Playing,
                CurrentEventId = event1.Id
            };

            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.NextTransmissionList();

            mockScheduler.Verify(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), 0), Times.Once);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void NextTransmissionList_WhileListIsPlaying_DeliversDeviceLists()
        {
            var mockDevice = new Mock<IDevice>();
            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(mockDevice.Object);
            TransmissionListEvent event2 = GenerateTestTransmissionListEvent(mockDevice.Object);
            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1, event2 })
            {
                State = TransmissionListState.Playing,
                CurrentEventId = event1.Id
            };

            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.NextTransmissionList();

            mockDevice.VerifySet(mock => mock.ActiveList = It.IsAny<DeviceList>());
        }

        #endregion

        #region DeviceList Subscription
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenTransmissionListSet_ShouldSubscribeToAllRelevantDevices()
        {
            var mockEventWatcher = new Mock<IDeviceListEventWatcher>();
            var mockDevice1 = new Mock<IDevice>().Object;
            var mockDevice2 = new Mock<IDevice>().Object;

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(mockDevice1);
            TransmissionListEvent event2 = GenerateTestTransmissionListEvent(mockDevice2);

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1, event2 });
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, mockEventWatcher.Object, new Mock<IDeviceListEventStore>().Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            mockEventWatcher.Verify(mock => mock.SubcsribeToDevice(serviceUnderTest, mockDevice1));
            mockEventWatcher.Verify(mock => mock.SubcsribeToDevice(serviceUnderTest, mockDevice2));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenTransmissionListSet_ShouldUnsubscribeFromAllDevicesFromPreviousList()
        {
            var mockEventWatcher = new Mock<IDeviceListEventWatcher>();
            var mockDevice1 = new Mock<IDevice>().Object;
            var mockDevice2 = new Mock<IDevice>().Object;

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(mockDevice1);
            TransmissionListEvent event2 = GenerateTestTransmissionListEvent(mockDevice2);

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1, event2 });
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, mockEventWatcher.Object, new Mock<IDeviceListEventStore>().Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            TransmissionListEvent event3 = GenerateTestTransmissionListEvent(new Mock<IDevice>().Object);
            TransmissionListEvent event4 = GenerateTestTransmissionListEvent(new Mock<IDevice>().Object);

            TransmissionList replacementList = new TransmissionList(new List<TransmissionListEvent>() { event3, event4 });
            serviceUnderTest.TransmissionList = replacementList;

            mockEventWatcher.Verify(mock => mock.UnsubcsribeFromDevice(serviceUnderTest, mockDevice1));
            mockEventWatcher.Verify(mock => mock.UnsubcsribeFromDevice(serviceUnderTest, mockDevice2));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenOneDeviceListEventCued_ShouldChangeTransmissionListEventToCueing()
        {
            DeviceListEvent deviceEvent1 = new DeviceListEvent("");
            DeviceListEvent deviceEvent2 = new DeviceListEvent("");

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(new Mock<IDevice>().Object, new Mock<IDevice>().Object);
            event1.EventFeatures[0].DeviceListEventId = deviceEvent1.Id;
            event1.EventFeatures[1].DeviceListEventId = deviceEvent2.Id;

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1 });
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, new Mock<IDeviceListEventWatcher>().Object, new Mock<IDeviceListEventStore>().Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent1.Id, event1.Id, new DeviceListEventState() { CurrentStatus = DeviceListEventStatus.CUED });

            Assert.Equal(TransmissionListEventState.Status.CUEING, event1.EventState.CurrentStatus);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenAllDeviceListEventsCued_ShouldChangeTransmissionListEventToCued()
        {
            DeviceListEvent deviceEvent1 = new DeviceListEvent("");
            DeviceListEvent deviceEvent2 = new DeviceListEvent("");

            var mockDeviceEventStore = CreateMockDeviceListEventStore(deviceEvent1, deviceEvent2);

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(new Mock<IDevice>().Object, new Mock<IDevice>().Object);
            event1.EventFeatures[0].DeviceListEventId = deviceEvent1.Id;
            event1.EventFeatures[1].DeviceListEventId = deviceEvent2.Id;

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1 });
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, new Mock<IDeviceListEventWatcher>().Object, mockDeviceEventStore.Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            deviceEvent1.EventState.CurrentStatus = DeviceListEventStatus.CUED;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent1.Id, event1.Id, new DeviceListEventState() { CurrentStatus = DeviceListEventStatus.CUED });
            deviceEvent2.EventState.CurrentStatus = DeviceListEventStatus.CUED;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent2.Id, event1.Id, new DeviceListEventState() { CurrentStatus = DeviceListEventStatus.CUED });

            Assert.Equal(TransmissionListEventState.Status.CUED, event1.EventState.CurrentStatus);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenAllDeviceListEventsCuedButNoListEventIdProvided_ShouldChangeTransmissionListEventToCued()
        {
            DeviceListEvent deviceEvent1 = new DeviceListEvent("");

            var mockDeviceEventStore = CreateMockDeviceListEventStore(deviceEvent1);

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(new Mock<IDevice>().Object);
            event1.EventFeatures[0].DeviceListEventId = deviceEvent1.Id;

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1 });
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, new Mock<IDeviceListEventWatcher>().Object, mockDeviceEventStore.Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            deviceEvent1.EventState.CurrentStatus = DeviceListEventStatus.CUED;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent1.Id, null, new DeviceListEventState() { CurrentStatus = DeviceListEventStatus.CUED });

            Assert.Equal(TransmissionListEventState.Status.CUED, event1.EventState.CurrentStatus);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenOneDeviceListEventPlaying_ShouldChangeTransmissionListEventToPlaying()
        {
            DeviceListEvent deviceEvent1 = new DeviceListEvent("");
            DeviceListEvent deviceEvent2 = new DeviceListEvent("");

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(new Mock<IDevice>().Object, new Mock<IDevice>().Object);
            event1.EventFeatures[0].DeviceListEventId = deviceEvent1.Id;
            event1.EventFeatures[1].DeviceListEventId = deviceEvent2.Id;

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1 });
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, new Mock<IDeviceListEventWatcher>().Object, new Mock<IDeviceListEventStore>().Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent1.Id, event1.Id, new DeviceListEventState() { CurrentStatus = DeviceListEventStatus.PLAYING });

            Assert.Equal(TransmissionListEventState.Status.PLAYING, event1.EventState.CurrentStatus);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenOneDeviceListEventPlaying_ShouldSetCurrentEventId()
        {
            DeviceListEvent deviceEvent1 = new DeviceListEvent("");
            DeviceListEvent deviceEvent2 = new DeviceListEvent("");

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(new Mock<IDevice>().Object, new Mock<IDevice>().Object);
            event1.EventFeatures[0].DeviceListEventId = deviceEvent1.Id;
            event1.EventFeatures[1].DeviceListEventId = deviceEvent2.Id;

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1 });
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, new Mock<IDeviceListEventWatcher>().Object, new Mock<IDeviceListEventStore>().Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent1.Id, event1.Id, new DeviceListEventState() { CurrentStatus = DeviceListEventStatus.PLAYING });

            Assert.Equal(event1.Id, transmissionList.CurrentEventId);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenOneDeviceListEventPlaying_ShouldSetTransmissionListEventActualStartTime()
        {
            DeviceListEvent deviceEvent1 = new DeviceListEvent("");
            DeviceListEvent deviceEvent2 = new DeviceListEvent("");

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(new Mock<IDevice>().Object, new Mock<IDevice>().Object);
            event1.EventFeatures[0].DeviceListEventId = deviceEvent1.Id;
            event1.EventFeatures[1].DeviceListEventId = deviceEvent2.Id;

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1 });
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, new Mock<IDeviceListEventWatcher>().Object, new Mock<IDeviceListEventStore>().Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            DateTimeOffset earliestStartTime = TimeSource.Now;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent1.Id, event1.Id, new DeviceListEventState() { CurrentStatus = DeviceListEventStatus.PLAYING });
            DateTimeOffset latestStartTime = TimeSource.Now;

            Assert.NotNull(event1.ActualStartTime);
            Assert.True(event1.ActualStartTime.Value >= earliestStartTime && event1.ActualStartTime.Value <= latestStartTime);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenAllDeviceListEventsPlayed_ShouldChangeTransmissionListEventToPlayed()
        {
            DeviceListEvent deviceEvent1 = new DeviceListEvent("");
            DeviceListEvent deviceEvent2 = new DeviceListEvent("");

            var mockDeviceEventStore = CreateMockDeviceListEventStore(deviceEvent1, deviceEvent2);

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(new Mock<IDevice>().Object, new Mock<IDevice>().Object);
            event1.EventFeatures[0].DeviceListEventId = deviceEvent1.Id;
            event1.EventFeatures[1].DeviceListEventId = deviceEvent2.Id;

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1 });
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, new Mock<IDeviceListEventWatcher>().Object, mockDeviceEventStore.Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            deviceEvent1.EventState.CurrentStatus = DeviceListEventStatus.PLAYED;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent1.Id, null, new DeviceListEventState() { CurrentStatus = DeviceListEventStatus.PLAYED });
            deviceEvent2.EventState.CurrentStatus = DeviceListEventStatus.PLAYED;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent2.Id, null, new DeviceListEventState() { CurrentStatus = DeviceListEventStatus.PLAYED });

            Assert.Equal(TransmissionListEventState.Status.PLAYED, event1.EventState.CurrentStatus);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenAllDeviceListEventsPlayed_ShouldSetTransmissionListEventActualEndTime()
        {
            DeviceListEvent deviceEvent1 = new DeviceListEvent("");
            DeviceListEvent deviceEvent2 = new DeviceListEvent("");

            var mockDeviceEventStore = CreateMockDeviceListEventStore(deviceEvent1, deviceEvent2);

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(new Mock<IDevice>().Object, new Mock<IDevice>().Object);
            event1.EventFeatures[0].DeviceListEventId = deviceEvent1.Id;
            event1.EventFeatures[1].DeviceListEventId = deviceEvent2.Id;

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1 });
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, new Mock<IDeviceListEventWatcher>().Object, mockDeviceEventStore.Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            deviceEvent1.EventState.CurrentStatus = DeviceListEventStatus.PLAYED;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent1.Id, null, new DeviceListEventState() { CurrentStatus = DeviceListEventStatus.PLAYED });
            deviceEvent2.EventState.CurrentStatus = DeviceListEventStatus.PLAYED;
            DateTimeOffset earliestEndTime = TimeSource.Now;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent2.Id, null, new DeviceListEventState() { CurrentStatus = DeviceListEventStatus.PLAYED });
            DateTimeOffset latestEndTime = TimeSource.Now;

            Assert.NotNull(event1.ActualEndTime);
            Assert.True(event1.ActualEndTime.Value >= earliestEndTime && event1.ActualEndTime.Value <= latestEndTime);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenLastEventSetToPlayed_ShouldChangeTransmissionListStateToStopped()
        {
            DeviceListEvent deviceEvent1 = new DeviceListEvent("");
            DeviceListEvent deviceEvent2 = new DeviceListEvent("");

            var mockDeviceEventStore = CreateMockDeviceListEventStore(deviceEvent1, deviceEvent2);

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(new Mock<IDevice>().Object, new Mock<IDevice>().Object);
            event1.EventFeatures[0].DeviceListEventId = deviceEvent1.Id;
            event1.EventFeatures[1].DeviceListEventId = deviceEvent2.Id;

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1 });
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, new Mock<IDeviceListEventWatcher>().Object, mockDeviceEventStore.Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            deviceEvent1.EventState.CurrentStatus = DeviceListEventStatus.PLAYED;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent1.Id, null, new DeviceListEventState() { CurrentStatus = DeviceListEventStatus.PLAYED });
            deviceEvent2.EventState.CurrentStatus = DeviceListEventStatus.PLAYED;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent2.Id, null, new DeviceListEventState() { CurrentStatus = DeviceListEventStatus.PLAYED });

            Assert.Equal(TransmissionListState.Stopped, transmissionList.State);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenOneDeviceListEventsPlayed_ShouldKeepTransmissionListEventAsPlaying()
        {
            DeviceListEvent deviceEvent1 = new DeviceListEvent("");
            DeviceListEvent deviceEvent2 = new DeviceListEvent("");

            var mockDeviceEventStore = CreateMockDeviceListEventStore(deviceEvent1, deviceEvent2);

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(new Mock<IDevice>().Object, new Mock<IDevice>().Object);
            event1.EventFeatures[0].DeviceListEventId = deviceEvent1.Id;
            event1.EventFeatures[1].DeviceListEventId = deviceEvent2.Id;

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1 });
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, new Mock<IDeviceListEventWatcher>().Object, mockDeviceEventStore.Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            deviceEvent2.EventState.CurrentStatus = DeviceListEventStatus.PLAYING;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent2.Id, event1.Id, new DeviceListEventState() { CurrentStatus = DeviceListEventStatus.PLAYING });
            deviceEvent1.EventState.CurrentStatus = DeviceListEventStatus.PLAYED;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent1.Id, event1.Id, new DeviceListEventState() { CurrentStatus = DeviceListEventStatus.PLAYED });

            Assert.Equal(TransmissionListEventState.Status.PLAYING, event1.EventState.CurrentStatus);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void OnTransmissionListChanged_WhenListIsPlaying_DeliversDeviceLists()
        {
            var mockDevice = new Mock<IDevice>();

            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent(""), new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            TransmissionList transmissionList = new TransmissionList()
            {
                State = TransmissionListState.Playing,
                Events = new List<TransmissionListEvent>() { GenerateTestTransmissionListEvent(new Mock<IDevice>().Object, new Mock<IDevice>().Object) }
            };
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.OnTransmissionListChanged();

            mockDevice.VerifySet(mock => mock.ActiveList = It.IsAny<DeviceList>());
        }
        #endregion

        #region OnTransmissionListChanged
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void OnTransmissionListChanged_WhenListIsEmpty_SetsStateToStoppedAndSetCurrentEventToNull()
        {
            var mockDevice = new Mock<IDevice>();

            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent(""), new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            TransmissionList transmissionList = new TransmissionList()
            {
                State = TransmissionListState.Playing
            };
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.OnTransmissionListChanged();

            Assert.Equal(TransmissionListState.Stopped, transmissionList.State);
            Assert.Null(transmissionList.CurrentEventId);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void OnTransmissionListChanged_WhenListContainsAnEventAndCurrentEventWasNull_SetsCurrentEventToAddedEvent()
        {
            var mockDevice = new Mock<IDevice>();

            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent(""), new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(mockDevice.Object);
            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1 });
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.OnTransmissionListChanged();

            Assert.Equal(event1.Id, transmissionList.CurrentEventId);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void OnTransmissionListChanged_WhenCurrentEventIsNoLongerTheNextEventAndListStopped_SetsCurrentEventToFirstEvent()
        {
            var mockDevice = new Mock<IDevice>();

            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent(""), new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            TransmissionListEvent event1 = new TransmissionListEvent(null, new List<IEventFeature>()) { Id = 1 };
            TransmissionListEvent event2 = new TransmissionListEvent(null, new List<IEventFeature>()) { Id = 2 };
            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1, event2 })
            {
                State = TransmissionListState.Stopped,
                CurrentEventId = event2.Id
            };
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.OnTransmissionListChanged();

            Assert.Equal(event1.Id, transmissionList.CurrentEventId);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void OnTransmissionListChanged_WhenCurrentEventIsNoLongerTheNextEventAndListStoppedAndPrecedingEventsArePlayed_SetsCurrentEventToNextEventToPlay()
        {
            var mockDevice = new Mock<IDevice>();

            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent(""), new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            TransmissionListEvent event1 = new TransmissionListEvent(null, new List<IEventFeature>()) { Id = 1 };
            event1.EventState.CurrentStatus = TransmissionListEventState.Status.PLAYED;
            TransmissionListEvent event2 = new TransmissionListEvent(null, new List<IEventFeature>()) { Id = 2 };
            event2.EventState.CurrentStatus = TransmissionListEventState.Status.PLAYED;
            TransmissionListEvent event3 = new TransmissionListEvent(null, new List<IEventFeature>()) { Id = 3 };
            event3.EventState.CurrentStatus = TransmissionListEventState.Status.CUED;
            TransmissionListEvent event4 = new TransmissionListEvent(null, new List<IEventFeature>()) { Id = 4 };
            event4.EventState.CurrentStatus = TransmissionListEventState.Status.CUED;
            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1, event2, event3, event4 })
            {
                State = TransmissionListState.Stopped,
                CurrentEventId = event4.Id
            };
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.OnTransmissionListChanged(2);

            Assert.Equal(event3.Id, transmissionList.CurrentEventId);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void OnTransmissionListChanged_WhenCurrentEventWasDeleted_SetsCurrentEventToNextEventToPlay()
        {
            var mockDevice = new Mock<IDevice>();

            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent(""), new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            TransmissionListEvent event1 = new TransmissionListEvent(null, new List<IEventFeature>()) { Id = 1 };
            event1.EventState.CurrentStatus = TransmissionListEventState.Status.PLAYED;
            TransmissionListEvent event2 = new TransmissionListEvent(null, new List<IEventFeature>()) { Id = 2 };
            event2.EventState.CurrentStatus = TransmissionListEventState.Status.PLAYED;
            TransmissionListEvent event4 = new TransmissionListEvent(null, new List<IEventFeature>()) { Id = 4 };
            event4.EventState.CurrentStatus = TransmissionListEventState.Status.CUED;
            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1, event2, event4 })
            {
                State = TransmissionListState.Stopped,
                CurrentEventId = 3
            };
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.OnTransmissionListChanged(2);

            Assert.Equal(event4.Id, transmissionList.CurrentEventId);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void OnTransmissionListChanged_WhenListIsNotPlaying_DoesNotDeliversDeviceLists()
        {
            var mockDevice = new Mock<IDevice>();

            Dictionary<IDevice, DeviceList> deviceLists = new Dictionary<IDevice, DeviceList>()
            {
                [mockDevice.Object] = new DeviceList(new List<DeviceListEvent>() { new DeviceListEvent(""), new DeviceListEvent("") })
            };
            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventStore>(), It.IsAny<int>())).Returns(deviceLists);

            TransmissionList transmissionList = new TransmissionList();
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object,
                                                                                         new Mock<IDeviceListEventWatcher>().Object,
                                                                                         new Mock<IDeviceListEventStore>().Object,
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.OnTransmissionListChanged();

            mockDevice.VerifySet(mock => mock.ActiveList = It.IsAny<DeviceList>(), Times.Never);
        }
        #endregion
    }
}
