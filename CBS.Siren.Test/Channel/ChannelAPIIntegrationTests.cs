﻿using CBS.Siren.PresentationLayer.DTOs;
using CBS.Siren.Test.DataLayer;
using CBS.Siren.Utilities;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CBS.Siren.Test
{

    public class ChannelAPIIntegrationTests
    {
        private readonly ITestOutputHelper _output;
        private const string ApiRoute = "api/1/automation/channel";

        public ChannelAPIIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        #region Get
        [Fact]
        [Trait("TestType","IntegrationTest")]
        public async Task GetChannels_WhenNoChannelsExist_ReturnsCollectionOfChannels()
        {
            WebApplicationFactoryBuilder<Startup> factoryBuilder = new WebApplicationFactoryBuilder<Startup>();

            using WebApplicationFactory<Startup> factory = factoryBuilder.CreateWebApplicationFactory(typeof(NullDataLayerInitializer));
            using HttpClient clientUnderTest = factory.CreateClient();

            HttpResponseMessage response = await clientUnderTest.GetAsync(ApiRoute);

            string content = await response.Content.ReadAsStringAsync();
            List<ChannelDTO> returnedChannels = content.DeserializeJson<List<ChannelDTO>>();

            Assert.Empty(returnedChannels);
        }
        
        [Fact]
        [Trait("TestType","IntegrationTest")]
        public async Task GetChannels_WhenChannelsExist_ReturnsCollectionOfChannels()
        {
            WebApplicationFactoryBuilder<Startup> factoryBuilder = new WebApplicationFactoryBuilder<Startup>();

            using WebApplicationFactory<Startup> factory = factoryBuilder.CreateWebApplicationFactory(typeof(ChannelDataLayerInitializer));
            using HttpClient clientUnderTest = factory.CreateClient();

            HttpResponseMessage response = await clientUnderTest.GetAsync(ApiRoute);

            string content = await response.Content.ReadAsStringAsync();
            List<ChannelDTO> returnedChannels = content.DeserializeJson<List<ChannelDTO>>();

            Assert.Single(returnedChannels);
            Assert.Equal("TestChannel", returnedChannels[0].Name);
        }
        #endregion

        #region GetById
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task GetChannelById_WhenIdDoesNotExist_ReturnsNotFound()
        {
            WebApplicationFactoryBuilder<Startup> factoryBuilder = new WebApplicationFactoryBuilder<Startup>();

            using WebApplicationFactory<Startup> factory = factoryBuilder.CreateWebApplicationFactory(typeof(ChannelDataLayerInitializer));
            using HttpClient clientUnderTest = factory.CreateClient();

            string route = $"{ApiRoute}/1000";
            HttpResponseMessage response = await clientUnderTest.GetAsync(route);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task GetChannelById_WhenIdDoesExists_ReturnsChannelDetails()
        {
            WebApplicationFactoryBuilder<Startup> factoryBuilder = new WebApplicationFactoryBuilder<Startup>();

            using WebApplicationFactory<Startup> factory = factoryBuilder.CreateWebApplicationFactory(typeof(ChannelDataLayerInitializer));
            using HttpClient clientUnderTest = factory.CreateClient();

            string route = $"{ApiRoute}/1";
            HttpResponseMessage response = await clientUnderTest.GetAsync(route);
            
            string content = await response.Content.ReadAsStringAsync();
            ChannelDetailsDTO returnedChannel = content.DeserializeJson<ChannelDetailsDTO>();

            Assert.NotNull(returnedChannel);
            Assert.Equal(1, returnedChannel.Id);
            Assert.Equal("TestChannel", returnedChannel.Name);
        }
        #endregion
    }
}
