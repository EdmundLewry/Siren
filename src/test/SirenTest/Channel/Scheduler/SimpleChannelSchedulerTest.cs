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
        TransmissionList list;
        PlayoutChainConfiguration config;
        Mock<IDevice> mockDevice;
        TransmissionEvent transmissionEvent;
        
        Mock<IEventTimingStrategy> mockTimingStrategy;

        public SimpleChannelSchedulerTests()
        {
            scheduler = new SimpleChannelScheduler();

            mockTimingStrategy = new Mock<IEventTimingStrategy>();
            transmissionEvent = new TransmissionEvent(new Mock<ISourceStrategy>().Object, new Mock<IPlayoutStrategy>().Object, mockTimingStrategy.Object);
            list = new TransmissionList(new List<TransmissionEvent>() {transmissionEvent});
            
            mockDevice = new Mock<IDevice>();
            config = new PlayoutChainConfiguration(new List<IDevice>() { mockDevice.Object });
        }

        [Fact]
        public void GenerateList_AssignsEventsToDevices()
        {
            ChannelList generatedList = scheduler.GenerateChannelList(list, config);

            Assert.Single(generatedList.Events);

            Assert.Equal(mockDevice.Object, generatedList.Events[0].Device);
            Assert.Equal(transmissionEvent, generatedList.Events[0].RelatedTransmissionEvent);
        }

        [Fact]
        public void GenerateList_TriggersEventTimingStrategyInEvents()
        {
            mockTimingStrategy.Setup(mock => mock.CalculateStartTime()).Returns(DateTime.Now);

            ChannelList generatedList = scheduler.GenerateChannelList(list, config);

            Assert.Single(generatedList.Events);

            mockTimingStrategy.Verify(mock => mock.CalculateStartTime(), Times.Once());
        }
    }
}