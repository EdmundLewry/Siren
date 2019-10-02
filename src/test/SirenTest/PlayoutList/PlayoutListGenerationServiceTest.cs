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

        public PlayoutListGenerationServiceTest()
        {
            mockDevice1 = new Mock<IDevice>();
            mockDevice2 = new Mock<IDevice>();
            
            TransmissionEvent transmissionEvent1 = new TransmissionEvent(new Mock<ISourceStrategy>().Object, new Mock<IPlayoutStrategy>().Object, new Mock<IEventTimingStrategy>().Object);
            ChannelListEvent event1 = new ChannelListEvent(transmissionEvent1, mockDevice1.Object);
            
            TransmissionEvent transmissionEvent2 = new TransmissionEvent(new Mock<ISourceStrategy>().Object, new Mock<IPlayoutStrategy>().Object, new Mock<IEventTimingStrategy>().Object);
            ChannelListEvent event2 = new ChannelListEvent(transmissionEvent2, mockDevice1.Object);
            
            TransmissionEvent transmissionEvent3 = new TransmissionEvent(new Mock<ISourceStrategy>().Object, new Mock<IPlayoutStrategy>().Object, new Mock<IEventTimingStrategy>().Object);
            ChannelListEvent event3 = new ChannelListEvent(transmissionEvent3, mockDevice2.Object);
            
            TransmissionEvent transmissionEvent4 = new TransmissionEvent(new Mock<ISourceStrategy>().Object, new Mock<IPlayoutStrategy>().Object, new Mock<IEventTimingStrategy>().Object);
            ChannelListEvent event4 = new ChannelListEvent(transmissionEvent4, mockDevice1.Object);
            
            ChannelList channelList = new ChannelList(new List<ChannelListEvent>() {event1, event2, event3, event4});
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

            //Need to verify that the list for device one contains 1,2,4
            //Need to verify that the list for device two contains 3
            
            //Assert.Equal()
        }
    }
}