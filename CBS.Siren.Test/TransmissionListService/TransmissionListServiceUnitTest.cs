using CBS.Siren.Device;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace CBS.Siren.Test
{
    public class TransmissionListServiceUnitTest
    {
        private TransmissionListEvent GenerateTestTransmissionListEvent(params IDevice[] devices)
        {
            List<IEventFeature> eventFeatures = new List<IEventFeature>();

            foreach(IDevice device in devices)
            {
                var eventFeature = new Mock<IEventFeature>();
                eventFeature.Setup(mock => mock.Device).Returns(device);
                eventFeatures.Add(eventFeature.Object);
            }

            return new TransmissionListEvent(new Mock<IEventTimingStrategy>().Object, eventFeatures);
        }

        private Mock<IDeviceListEventFactory> CreateMockDeviceListEventFactory(params DeviceListEvent[] deviceEvents)
        {
            var mockDeviceEventFactory = new Mock<IDeviceListEventFactory>();
            foreach(DeviceListEvent deviceListEvent in deviceEvents)
            {
                mockDeviceEventFactory.Setup(mock => mock.GetEventById(deviceListEvent.Id)).Returns(deviceListEvent);
            }

            return mockDeviceEventFactory;
        }

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
            mockScheduler.Setup(mock => mock.ScheduleTransmissionList(It.IsAny<TransmissionList>(), It.IsAny<IDeviceListEventFactory>())).Returns(deviceLists);

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>(), null);
            using TransmissionListService serviceUnderTest = new TransmissionListService(mockScheduler.Object, 
                                                                                         new Mock<IDeviceListEventWatcher>().Object, 
                                                                                         new Mock<IDeviceListEventFactory>().Object, 
                                                                                         new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.PlayTransmissionList();

            mockDevice.VerifySet(mock => mock.ActiveList = It.IsAny<DeviceList>());
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenTransmissionListSet_ShouldSubscribeToAllRelevantDevices()
        {
            var mockEventWatcher = new Mock<IDeviceListEventWatcher>();
            var mockDevice1 = new Mock<IDevice>().Object;
            var mockDevice2 = new Mock<IDevice>().Object;

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent(mockDevice1);
            TransmissionListEvent event2 = GenerateTestTransmissionListEvent(mockDevice2);

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1, event2 }, null);
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, mockEventWatcher.Object, new Mock<IDeviceListEventFactory>().Object, new Mock<ILogger<TransmissionListService>>().Object)
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

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1, event2 }, null);
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, mockEventWatcher.Object, new Mock<IDeviceListEventFactory>().Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            TransmissionListEvent event3 = GenerateTestTransmissionListEvent(new Mock<IDevice>().Object);
            TransmissionListEvent event4 = GenerateTestTransmissionListEvent(new Mock<IDevice>().Object);

            TransmissionList replacementList = new TransmissionList(new List<TransmissionListEvent>() { event3, event4 }, null);
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

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent();
            event1.RelatedDeviceListEvents.Add(deviceEvent1.Id);
            event1.RelatedDeviceListEvents.Add(deviceEvent2.Id);

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1 }, null);
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, new Mock<IDeviceListEventWatcher>().Object, new Mock<IDeviceListEventFactory>().Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent1.Id, new DeviceListEventState() { CurrentStatus = DeviceListEventState.Status.CUED });

            Assert.Equal(TransmissionListEventState.Status.CUEING, event1.EventState.CurrentStatus);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenAllDeviceListEventsCued_ShouldChangeTransmissionListEventToCued()
        {
            DeviceListEvent deviceEvent1 = new DeviceListEvent("");
            DeviceListEvent deviceEvent2 = new DeviceListEvent("");

            var mockDeviceEventFactory = CreateMockDeviceListEventFactory(deviceEvent1, deviceEvent2);

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent();
            event1.RelatedDeviceListEvents.Add(deviceEvent1.Id);
            event1.RelatedDeviceListEvents.Add(deviceEvent2.Id);

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1 }, null);
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, new Mock<IDeviceListEventWatcher>().Object, mockDeviceEventFactory.Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            deviceEvent1.EventState.CurrentStatus = DeviceListEventState.Status.CUED;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent1.Id, new DeviceListEventState() { CurrentStatus = DeviceListEventState.Status.CUED });
            deviceEvent2.EventState.CurrentStatus = DeviceListEventState.Status.CUED;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent2.Id, new DeviceListEventState() { CurrentStatus = DeviceListEventState.Status.CUED });

            Assert.Equal(TransmissionListEventState.Status.CUED, event1.EventState.CurrentStatus);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenOneDeviceListEventPlaying_ShouldChangeTransmissionListEventToPlaying()
        {
            DeviceListEvent deviceEvent1 = new DeviceListEvent("");
            DeviceListEvent deviceEvent2 = new DeviceListEvent("");

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent();
            event1.RelatedDeviceListEvents.Add(deviceEvent1.Id);
            event1.RelatedDeviceListEvents.Add(deviceEvent2.Id);

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1 }, null);
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, new Mock<IDeviceListEventWatcher>().Object, new Mock<IDeviceListEventFactory>().Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent1.Id, new DeviceListEventState() { CurrentStatus = DeviceListEventState.Status.PLAYING });

            Assert.Equal(TransmissionListEventState.Status.PLAYING, event1.EventState.CurrentStatus);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenAllDeviceListEventsPlayed_ShouldChangeTransmissionListEventToPlayed()
        {
            DeviceListEvent deviceEvent1 = new DeviceListEvent("");
            DeviceListEvent deviceEvent2 = new DeviceListEvent("");
            
            var mockDeviceEventFactory = CreateMockDeviceListEventFactory(deviceEvent1, deviceEvent2);

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent();
            event1.RelatedDeviceListEvents.Add(deviceEvent1.Id);
            event1.RelatedDeviceListEvents.Add(deviceEvent2.Id);

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1 }, null);
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, new Mock<IDeviceListEventWatcher>().Object, mockDeviceEventFactory.Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            deviceEvent1.EventState.CurrentStatus = DeviceListEventState.Status.PLAYED;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent1.Id, new DeviceListEventState() { CurrentStatus = DeviceListEventState.Status.PLAYED });
            deviceEvent2.EventState.CurrentStatus = DeviceListEventState.Status.PLAYED;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent2.Id, new DeviceListEventState() { CurrentStatus = DeviceListEventState.Status.PLAYED });

            Assert.Equal(TransmissionListEventState.Status.PLAYED, event1.EventState.CurrentStatus);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void TransmissionListService_WhenOneDeviceListEventsPlayed_ShouldKeepTransmissionListEventAsPlaying()
        {
            DeviceListEvent deviceEvent1 = new DeviceListEvent("");
            DeviceListEvent deviceEvent2 = new DeviceListEvent("");

            var mockDeviceEventFactory = CreateMockDeviceListEventFactory(deviceEvent1, deviceEvent2);

            TransmissionListEvent event1 = GenerateTestTransmissionListEvent();
            event1.RelatedDeviceListEvents.Add(deviceEvent1.Id);
            event1.RelatedDeviceListEvents.Add(deviceEvent2.Id);

            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>() { event1 }, null);
            using TransmissionListService serviceUnderTest = new TransmissionListService(new Mock<IScheduler>().Object, new Mock<IDeviceListEventWatcher>().Object, mockDeviceEventFactory.Object, new Mock<ILogger<TransmissionListService>>().Object)
            {
                TransmissionList = transmissionList
            };

            deviceEvent2.EventState.CurrentStatus = DeviceListEventState.Status.PLAYING;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent2.Id, new DeviceListEventState() { CurrentStatus = DeviceListEventState.Status.PLAYING });
            deviceEvent1.EventState.CurrentStatus = DeviceListEventState.Status.PLAYED;
            serviceUnderTest.OnDeviceListEventStatusChanged(deviceEvent1.Id, new DeviceListEventState() { CurrentStatus = DeviceListEventState.Status.PLAYED });

            Assert.Equal(TransmissionListEventState.Status.PLAYING, event1.EventState.CurrentStatus);
        }
    }
}
