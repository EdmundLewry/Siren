using Xunit;
using Moq;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;

using PBS.Siren;

namespace SirenTest
{
    public class PlayoutListGenerationServiceTest
    {
        Mock<IDevice> mockDevice1;
        Mock<IDevice> mockDevice2;
        ChannelList channelList;

        PlaylistEvent PlaylistEvent1;
        PlaylistEvent PlaylistEvent2;
        PlaylistEvent PlaylistEvent3;
        PlaylistEvent PlaylistEvent4;

        public PlayoutListGenerationServiceTest()
        {
            mockDevice1 = new Mock<IDevice>();
            mockDevice2 = new Mock<IDevice>();
            
            var mockSourceStrategy = new Mock<ISourceStrategy>();
            var mockPlayoutStrategy = new Mock<IPlayoutStrategy>();
            var mockEventTimingStrategy = new Mock<IEventTimingStrategy>();
            PlaylistEvent1 = new PlaylistEvent(mockSourceStrategy.Object, mockPlayoutStrategy.Object, mockEventTimingStrategy.Object);
            ChannelListEvent event1 = new ChannelListEvent(PlaylistEvent1, mockDevice1.Object);
            
            PlaylistEvent2 = new PlaylistEvent(mockSourceStrategy.Object, mockPlayoutStrategy.Object, mockEventTimingStrategy.Object);
            ChannelListEvent event2 = new ChannelListEvent(PlaylistEvent2, mockDevice1.Object);
            
            PlaylistEvent3 = new PlaylistEvent(mockSourceStrategy.Object, mockPlayoutStrategy.Object, mockEventTimingStrategy.Object);
            ChannelListEvent event3 = new ChannelListEvent(PlaylistEvent3, mockDevice2.Object);
            
            PlaylistEvent4 = new PlaylistEvent(mockSourceStrategy.Object, mockPlayoutStrategy.Object, mockEventTimingStrategy.Object);
            ChannelListEvent event4 = new ChannelListEvent(PlaylistEvent4, mockDevice1.Object);
            
            channelList = new ChannelList(new List<ChannelListEvent>() {event1, event2, event3, event4});
        }

        [Fact]
        public void GeneratePlayoutLists_ShouldCreateOnePlayoutListForEachDevice()
        {
            Dictionary<IDevice,DeviceList> lists = DeviceListGenerationService.GeneratePlayoutLists(channelList);

            Assert.True(lists.ContainsKey(mockDevice1.Object));
            Assert.True(lists.ContainsKey(mockDevice2.Object));
        }

        [Fact]
        public void GeneratePlayoutLists_ShouldCreateListsOfOnlyEventsRelatedToOneDevice()
        {
            Dictionary<IDevice,DeviceList> lists = DeviceListGenerationService.GeneratePlayoutLists(channelList);

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