using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using CBS.Siren.DTO;
using System.Linq;
using System;
using System.Text;
using Xunit.Abstractions;
using System.Net;

namespace CBS.Siren.Test
{
    public class TransmissionListIntegrationTests
    {
        private readonly ITestOutputHelper _output;
        public TransmissionListIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private TransmissionListEventCreationDTO GetListEventCreationDTO()
        {
            return new TransmissionListEventCreationDTO()
            {
                TimingData = new TimingStrategyCreationDTO()
                {
                    StrategyType = "fixed",
                    TargetStartTime = DateTime.Parse("2020-03-22 12:30:10")
                },
                Features = new List<ListEventFeatureCreationDTO>(){
                    new ListEventFeatureCreationDTO(){
                        FeatureType = "video",
                        PlayoutStrategy = new PlayoutStrategyCreationDTO() { StrategyType = "primaryVideo" },
                        SourceStrategy = new SourceStrategyCreationDTO() {
                            StrategyType = "mediaSource",
                            SOM = new TimeSpan(0,0,0,0,0),
                            EOM = new TimeSpan(0,0,0,30,0),
                            MediaName = "TestInstance"
                        }
                    }
                }
            };
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenTransmissionListsRequested_ReturnsCollectionOfTransmissionLists()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            HttpResponseMessage response = await clientUnderTest.GetAsync("api/1/transmissionlist");

            string content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Content read as: {content}, Response status code: {response.StatusCode}");
            List<TransmissionListDTO> returnedLists = JsonSerializer.Deserialize<List<TransmissionListDTO>>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            Assert.Single(returnedLists);
            //Attempt to parse id to ensure it's valid
            int.Parse(returnedLists.First().Id);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenTransmissionListEventsRequested_ReturnsCollectionOfTransmissionListEvents()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventCreationDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), UnicodeEncoding.UTF8, "application/json");

            _ = await clientUnderTest.PostAsync("api/1/transmissionlist/0/events", eventCreationData);

            HttpResponseMessage response = await clientUnderTest.GetAsync("api/1/transmissionlist/0/events");

            string content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Content read as: {content}, Response status code: {response.StatusCode}");
            List<TransmissionListEventDTO> returnedList = JsonSerializer.Deserialize<List<TransmissionListEventDTO>>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            Assert.NotEmpty(returnedList);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenAddEventIsCalled_ReturnsCreatedEvent()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventCreationDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), UnicodeEncoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/transmissionlist/0/events", eventCreationData);
            string content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Content read as: {content}, Response status code: {response.StatusCode}");
            TransmissionListEventDTO returnedEvent = JsonSerializer.Deserialize<TransmissionListEventDTO>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            Assert.Equal("UNSCHEDULED", returnedEvent.EventState);
            Assert.Equal(1, returnedEvent.EventFeatureCount);
            Assert.Equal(0, returnedEvent.RelatedDeviceListEventCount);
            Assert.Equal("fixed", returnedEvent.EventTimingStrategy);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenAddEventIsCalledWithBadListId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventCreationDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), UnicodeEncoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/transmissionlist/1000/events", eventCreationData);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenRemoveEventIsCalledWithBadListId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            HttpResponseMessage response = await clientUnderTest.DeleteAsync("api/1/transmissionlist/1000/events/0");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenRemoveEventIsCalledWithBadEventId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            HttpResponseMessage response = await clientUnderTest.DeleteAsync("api/1/transmissionlist/0/events/1000");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenRemoveEventIsCalled_RemovesEventFromList()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventCreationDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), UnicodeEncoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/transmissionlist/0/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            response = await clientUnderTest.DeleteAsync("api/1/transmissionlist/0/events/0");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await clientUnderTest.GetAsync("api/1/transmissionlist/0/events");

            string content = await response.Content.ReadAsStringAsync();
            List<TransmissionListEventDTO> returnedList = JsonSerializer.Deserialize<List<TransmissionListEventDTO>>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            Assert.Empty(returnedList);
        }
    }
}
