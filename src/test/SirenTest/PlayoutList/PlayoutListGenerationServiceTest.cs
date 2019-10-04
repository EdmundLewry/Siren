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

        TransmissionEvent transmissionEvent1;
        TransmissionEvent transmissionEvent2;
        TransmissionEvent transmissionEvent3;
        TransmissionEvent transmissionEvent4;

        public PlayoutListGenerationServiceTest()
        {
            mockDevice1 = new Mock<IDevice>();
            mockDevice2 = new Mock<IDevice>();
            
            var mockSourceStrategy = new Mock<ISourceStrategy>();
            var mockPlayoutStrategy = new Mock<IPlayoutStrategy>();
            var mockEventTimingStrategy = new Mock<IEventTimingStrategy>();
            transmissionEvent1 = new TransmissionEvent(mockSourceStrategy.Object, mockPlayoutStrategy.Object, mockEventTimingStrategy.Object);
            ChannelListEvent event1 = new ChannelListEvent(transmissionEvent1, mockDevice1.Object);
            
            transmissionEvent2 = new TransmissionEvent(mockSourceStrategy.Object, mockPlayoutStrategy.Object, mockEventTimingStrategy.Object);
            ChannelListEvent event2 = new ChannelListEvent(transmissionEvent2, mockDevice1.Object);
            
            transmissionEvent3 = new TransmissionEvent(mockSourceStrategy.Object, mockPlayoutStrategy.Object, mockEventTimingStrategy.Object);
            ChannelListEvent event3 = new ChannelListEvent(transmissionEvent3, mockDevice2.Object);
            
            transmissionEvent4 = new TransmissionEvent(mockSourceStrategy.Object, mockPlayoutStrategy.Object, mockEventTimingStrategy.Object);
            ChannelListEvent event4 = new ChannelListEvent(transmissionEvent4, mockDevice1.Object);
            
            channelList = new ChannelList(new List<ChannelListEvent>() {event1, event2, event3, event4});
        }

        [Fact]
        public void GeneratePlayoutLists_ShouldCreateOnePlayoutListForEachDevice()
        {
            Dictionary<IDevice,PlayoutList> lists = PlayoutListGenerationService.GeneratePlayoutLists(channelList);

            Assert.True(lists.ContainsKey(mockDevice1.Object));
            Assert.True(lists.ContainsKey(mockDevice2.Object));
        }

        [Fact]
        public void GeneratePlayoutLists_ShouldCreateListsOfOnlyEventsRelatedToOneDevice()
        {
            Dictionary<IDevice,PlayoutList> lists = PlayoutListGenerationService.GeneratePlayoutLists(channelList);

            PlayoutList deviceOneList = lists[mockDevice1.Object];
            Assert.Equal(3, deviceOneList.Events.Count);

            JObject eventDataJSON = JObject.Parse(deviceOneList.Events[0].EventData);
            Assert.Equal(transmissionEvent1.Id.ToString(), (string)eventDataJSON["Event"]["Id"]);
            eventDataJSON = JObject.Parse(deviceOneList.Events[1].EventData);
            Assert.Equal(transmissionEvent2.Id.ToString(), (string)eventDataJSON["Event"]["Id"]);
            eventDataJSON = JObject.Parse(deviceOneList.Events[2].EventData);
            Assert.Equal(transmissionEvent4.Id.ToString(), (string)eventDataJSON["Event"]["Id"]);

            PlayoutList deviceTwoList = lists[mockDevice2.Object];
            Assert.Single(deviceTwoList.Events);
            eventDataJSON = JObject.Parse(deviceTwoList.Events[0].EventData);
            Assert.Equal(transmissionEvent3.Id.ToString(), (string)eventDataJSON["Event"]["Id"]);
        }
    }
}