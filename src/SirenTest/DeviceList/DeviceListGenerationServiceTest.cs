using Xunit;
using Moq;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;

using PBS.Siren;

namespace SirenTest
{
    public class DeviceListGenerationServiceTest
    {
        Mock<IDevice> mockDevice1;
        Mock<IDevice> mockDevice2;
        TransmissionList channelList;

        PlaylistEvent PlaylistEvent1;
        PlaylistEvent PlaylistEvent2;
        PlaylistEvent PlaylistEvent3;
        PlaylistEvent PlaylistEvent4;

        //TODO:1 This should be the scheduler I think
        public DeviceListGenerationServiceTest()
        {
            mockDevice1 = new Mock<IDevice>();
            mockDevice2 = new Mock<IDevice>();
            
            var mockSourceStrategy = new Mock<ISourceStrategy>();
            var mockPlayoutStrategy = new Mock<IPlayoutStrategy>();
            var mockEventTimingStrategy = new Mock<IEventTimingStrategy>();
            PlaylistEvent1 = new PlaylistEvent(mockSourceStrategy.Object, mockPlayoutStrategy.Object, mockEventTimingStrategy.Object);
            TransmissionListEvent event1 = new TransmissionListEvent(PlaylistEvent1, mockDevice1.Object);
            
            PlaylistEvent2 = new PlaylistEvent(mockSourceStrategy.Object, mockPlayoutStrategy.Object, mockEventTimingStrategy.Object);
            TransmissionListEvent event2 = new TransmissionListEvent(PlaylistEvent2, mockDevice1.Object);
            
            PlaylistEvent3 = new PlaylistEvent(mockSourceStrategy.Object, mockPlayoutStrategy.Object, mockEventTimingStrategy.Object);
            TransmissionListEvent event3 = new TransmissionListEvent(PlaylistEvent3, mockDevice2.Object);
            
            PlaylistEvent4 = new PlaylistEvent(mockSourceStrategy.Object, mockPlayoutStrategy.Object, mockEventTimingStrategy.Object);
            TransmissionListEvent event4 = new TransmissionListEvent(PlaylistEvent4, mockDevice1.Object);
            
            channelList = new TransmissionList(new List<TransmissionListEvent>() {event1, event2, event3, event4});
        }

        [Fact]
        public void GenerateDeviceLists_ShouldCreateOneDeviceListForEachDevice()
        {
            Dictionary<IDevice,DeviceList> lists = DeviceListGenerationService.GenerateDeviceLists(channelList);

            Assert.True(lists.ContainsKey(mockDevice1.Object));
            Assert.True(lists.ContainsKey(mockDevice2.Object));
        }

        [Fact]
        public void GenerateDeviceLists_ShouldCreateListsOfOnlyEventsRelatedToOneDevice()
        {
            Dictionary<IDevice,DeviceList> lists = DeviceListGenerationService.GenerateDeviceLists(channelList);

            DeviceList deviceOneList = lists[mockDevice1.Object];
            Assert.Equal(3, deviceOneList.Events.Count);

            JObject eventDataJSON = JObject.Parse(deviceOneList.Events[0].EventData);
            Assert.Equal(PlaylistEvent1.Id.ToString(), (string)eventDataJSON["Event"]["Id"]);
            eventDataJSON = JObject.Parse(deviceOneList.Events[1].EventData);
            Assert.Equal(PlaylistEvent2.Id.ToString(), (string)eventDataJSON["Event"]["Id"]);
            eventDataJSON = JObject.Parse(deviceOneList.Events[2].EventData);
            Assert.Equal(PlaylistEvent4.Id.ToString(), (string)eventDataJSON["Event"]["Id"]);

            DeviceList deviceTwoList = lists[mockDevice2.Object];
            Assert.Single(deviceTwoList.Events);
            eventDataJSON = JObject.Parse(deviceTwoList.Events[0].EventData);
            Assert.Equal(PlaylistEvent3.Id.ToString(), (string)eventDataJSON["Event"]["Id"]);
        }
    }
}