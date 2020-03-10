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

        Mock<IVideoChain> mockChainConfig;
        Mock<IPlaylist> mockTransmissionList;
        Mock<IScheduler> mockScheduler;

        public ChannelTest()
        {
            mockChainConfig = new Mock<IVideoChain>();
            mockTransmissionList = new Mock<IPlaylist>();

            mockScheduler.Setup(mock => mock.GenerateChannelList(It.IsAny<IPlaylist>(), It.IsAny<IVideoChain>())).Returns(new TransmissionList(new List<TransmissionListEvent>(), mockTransmissionList.Object, null));
        }

        //[Fact]
        //public void Channel_CallsSchedulerGenerate()
        //{
        //    channel = new Channel(mockChainConfig.Object, mockTransmissionList.Object);

        //    mockScheduler.Verify(mock => mock.GenerateChannelList(mockTransmissionList.Object, mockChainConfig.Object), Times.Once());
        //}
    }
}