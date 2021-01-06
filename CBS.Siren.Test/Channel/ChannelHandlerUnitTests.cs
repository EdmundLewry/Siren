using CBS.Siren.Application;
using CBS.Siren.Data;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CBS.Siren.Test
{
    public class ChannelHandlerUnitTests
    {
        private readonly Mock<ILogger<ChannelHandler>> _logger;
        private readonly Mock<IDataLayer> _dataLayer;
        private readonly Channel _channel;

        public ChannelHandlerUnitTests()
        {
            _logger = new Mock<ILogger<ChannelHandler>>();
            _dataLayer = new Mock<IDataLayer>();

            _channel = new Channel()
            {
                Id = 1,
                Name = "TestChannel",
                TransmissionLists = new List<TransmissionList>()
            };
        }

        private ChannelHandler CreateHandlerUnderTest()
        {
            _dataLayer.Setup(mock => mock.Channels()).ReturnsAsync(new List<Channel>() { _channel});
            return new ChannelHandler(_logger.Object, _dataLayer.Object);
        }

        #region Get
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GetAllChannels_RequestsChannelsFromDataLayer()
        {
            ChannelHandler codeUnderTest = CreateHandlerUnderTest();
            _ = codeUnderTest.GetAllChannels();

            _dataLayer.Verify(mock => mock.Channels(), Times.Once);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task GetChannelById_WhenNoChannelExists_ThrowsException()
        {
            ChannelHandler codeUnderTest = CreateHandlerUnderTest();
            _ = codeUnderTest.GetAllChannels();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.GetChannelById(30));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task GetChannelById_WhenChannelExists_ReturnsChannel()
        {
            ChannelHandler codeUnderTest = CreateHandlerUnderTest();
            Channel retrievedChannel = await codeUnderTest.GetChannelById(1);

            Assert.Equal(1, retrievedChannel.Id);
            Assert.Equal("TestChannel", retrievedChannel.Name);
        }


        #endregion
    }
}
