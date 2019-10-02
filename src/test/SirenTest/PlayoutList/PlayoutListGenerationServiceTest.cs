using Xunit;
using Moq;

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

            //I think the next step to have the event data compiled into a single json document. This requires
            //functions on the strategies and transmission event
            //This json document can then be passed to the playout list event

            //This won't be ToString eventually. It'll be something like getEventData
            PlayoutList deviceOneList = lists[mockDevice1.Object];
            Assert.Equal(3, deviceOneList.Events.Count);
            Assert.Equal("Event 1", deviceOneList.Events[0].ToString());
            Assert.Equal("Event 2", deviceOneList.Events[1].ToString());
            Assert.Equal("Event 4", deviceOneList.Events[2].ToString());

            PlayoutList deviceTwoList = lists[mockDevice2.Object];
            Assert.Single(deviceOneList.Events);
            Assert.Equal("Event 3", deviceOneList.Events[0].ToString());
        }
    }
}