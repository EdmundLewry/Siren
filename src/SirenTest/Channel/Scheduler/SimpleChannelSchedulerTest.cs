using Xunit;
using Moq;

using System;
using System.Collections.Generic;

using PBS.Siren;

namespace SirenTest
{
    public class SimpleChannelSchedulerTests
    {
        SimpleChannelScheduler scheduler;
        Playlist list;
        PlayoutChainConfiguration config;
        Mock<IDevice> mockDevice;
        PlaylistEvent PlaylistEvent;
        
        Mock<IEventTimingStrategy> mockTimingStrategy;

        public SimpleChannelSchedulerTests()
        {
            scheduler = new SimpleChannelScheduler();

            mockTimingStrategy = new Mock<IEventTimingStrategy>();
            mockTimingStrategy.Setup(mock => mock.CalculateStartTime()).Returns(DateTime.Now);
            PlaylistEvent = new PlaylistEvent(new Mock<ISourceStrategy>().Object, new Mock<IPlayoutStrategy>().Object, mockTimingStrategy.Object);
            list = new Playlist(new List<PlaylistEvent>() {PlaylistEvent});
            
            mockDevice = new Mock<IDevice>();
            config = new PlayoutChainConfiguration(new List<IDevice>() { mockDevice.Object });
        }

        [Fact]
        public void GenerateList_AssignsEventsToDevices()
        {
            TransmissionList generatedList = scheduler.GenerateChannelList(list, config);

            Assert.Single(generatedList.Events);

            Assert.Equal(mockDevice.Object, generatedList.Events[0].Device);
            Assert.Equal(PlaylistEvent, generatedList.Events[0].RelatedPlaylistEvent);
        }

        [Fact]
        public void GenerateList_TriggersEventTimingStrategyInEvents()
        {
            TransmissionList generatedList = scheduler.GenerateChannelList(list, config);

            Assert.Single(generatedList.Events);

            mockTimingStrategy.Verify(mock => mock.CalculateStartTime(), Times.Once());
        }
    }
}