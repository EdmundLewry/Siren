using Xunit;
using Moq;

using System;
using System.Collections.Generic;

using PBS.Siren;

namespace SirenTest
{
    public class ChannelTest
    {
        Channel channel;

        Mock<IPlayoutChainConfiguration> mockChainConfig;
        Mock<ITransmissionList> mockTransmissionList;
        Mock<IScheduler> mockScheduler;

        public ChannelTest()
        {
            mockChainConfig = new Mock<IPlayoutChainConfiguration>();
            mockTransmissionList = new Mock<ITransmissionList>();
            mockScheduler = new Mock<IScheduler>();

            mockScheduler.Setup(mock => mock.GenerateChannelList(It.IsAny<ITransmissionList>(), It.IsAny<IPlayoutChainConfiguration>())).Returns(new ChannelList(new List<ChannelListEvent>()));
        }

        [Fact]
        public void Channel_CallsSchedulerGenerate()
        {
            channel = new Channel(mockChainConfig.Object, mockTransmissionList.Object, mockScheduler.Object);

            mockScheduler.Verify(mock => mock.GenerateChannelList(mockTransmissionList.Object, mockChainConfig.Object), Times.Once());
        }
    }
}