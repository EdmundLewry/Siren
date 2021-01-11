using CBS.Siren.Application;
using CBS.Siren.Data;
using CBS.Siren.Device;
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
            _dataLayer.Setup(mock => mock.AddUpdateChannels(It.IsAny<Channel[]>())).ReturnsAsync((Channel[] channels) => new List<Channel>() {
                channels[0]
            });
            return new ChannelHandler(_logger.Object, _dataLayer.Object, new Mock<IDeviceManager>().Object);
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

        #region Create
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task CreateChannel_WithNoChannelName_ThrowsException()
        {
            ChannelHandler codeUnderTest = CreateHandlerUnderTest();
            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.AddChannel(""));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task CreateChannel_WithExistingChannelName_ThrowsException()
        {
            ChannelHandler codeUnderTest = CreateHandlerUnderTest();
            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.AddChannel("TestChannel"));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task CreateChannel_WithNewChannelName_ReturnsChannel()
        {
            ChannelHandler codeUnderTest = CreateHandlerUnderTest();
            Channel channel = await codeUnderTest.AddChannel("NewChannel");

            Assert.Equal("NewChannel", channel.Name);
            Assert.Empty(channel.TransmissionLists);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task CreateChannel_WithNewChannelNames_AddsChannelToTheDataLayer()
        {
            ChannelHandler codeUnderTest = CreateHandlerUnderTest();
            _ = await codeUnderTest.AddChannel("NewChannel");

            _dataLayer.Verify(mock => mock.AddUpdateChannels(It.IsAny<Channel[]>()), Times.Once);
        }
        #endregion
    }
}
