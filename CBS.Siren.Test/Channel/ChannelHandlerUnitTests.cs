using CBS.Siren.Application;
using CBS.Siren.Data;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CBS.Siren.Test
{
    public class ChannelHandlerUnitTests
    {
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GetAllChannels_RequestsChannelsFromDataLayer()
        {
            Mock<IDataLayer> mockDataLayer = new Mock<IDataLayer>();
            Mock<ILogger<ChannelHandler>> logger = new Mock<ILogger<ChannelHandler>>();
            ChannelHandler codeUnderTest = new ChannelHandler(logger.Object, mockDataLayer.Object);

            _ = codeUnderTest.GetAllChannels();

            mockDataLayer.Verify(mock => mock.Channels(), Times.Once);
        }
    }
}
