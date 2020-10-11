using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using CBS.Siren.DTO;
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

        private TransmissionListEventUpsertDTO GetListEventCreationDTO()
        {
            return new TransmissionListEventUpsertDTO()
            {
                TimingData = new TimingStrategyUpsertDTO()
                {
                    StrategyType = "fixed",
                    TargetStartTime = DateTimeOffset.Parse("2020-03-22 12:30:10")
                },
                Features = new List<ListEventFeatureUpsertDTO>(){
                    new ListEventFeatureUpsertDTO(){
                        FeatureType = "video",
                        PlayoutStrategy = new PlayoutStrategyCreationDTO() { StrategyType = "primaryVideo" },
                        SourceStrategy = new SourceStrategyCreationDTO() {
                            StrategyType = "mediaSource",
                            SOM = "00:00:00:00",
                            EOM = "00:00:30:00",
                            MediaName = "TestInstance"
                        }
                    }
                }
            };
        }

        #region GetLists
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenTransmissionListsRequested_ReturnsCollectionOfTransmissionLists()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            HttpResponseMessage response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist");

            string content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Content read as: {content}, Response status code: {response.StatusCode}");
            List<TransmissionListDTO> returnedLists = JsonSerializer.Deserialize<List<TransmissionListDTO>>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            Assert.Single(returnedLists);
            Assert.Equal("Stopped", returnedLists[0].ListState);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task GetListById_WhenListIdInvalid_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            HttpResponseMessage response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1000");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenTransmissionListRequestedById_ReturnsTransmissionLists()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), UnicodeEncoding.UTF8, "application/json");

            _ = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            HttpResponseMessage response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1");

            string content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Content read as: {content}, Response status code: {response.StatusCode}");
            TransmissionListDetailDTO returnedList = JsonSerializer.Deserialize<TransmissionListDetailDTO>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            Assert.NotNull(returnedList);
            Assert.Equal("Stopped", returnedList.ListState);
            Assert.Single(returnedList.Events);
        }
        #endregion

        #region GetListEvents
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenTransmissionListEventsRequested_ReturnsCollectionOfTransmissionListEvents()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), UnicodeEncoding.UTF8, "application/json");

            _ = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            HttpResponseMessage response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1/events");

            string content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Content read as: {content}, Response status code: {response.StatusCode}");
            List<TransmissionListEventDTO> returnedList = JsonSerializer.Deserialize<List<TransmissionListEventDTO>>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            Assert.NotEmpty(returnedList);
        }
        
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenListIdInvalid_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            HttpResponseMessage response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1000/events");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region AddEvent
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenAddEventIsCalled_ReturnsCreatedEvent()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), UnicodeEncoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            string content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Content read as: {content}, Response status code: {response.StatusCode}");
            TransmissionListEventDTO returnedEvent = JsonSerializer.Deserialize<TransmissionListEventDTO>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            Assert.Equal("SCHEDULED", returnedEvent.EventState);
            Assert.Equal(1, returnedEvent.EventFeatureCount);
            Assert.Equal(1, returnedEvent.RelatedDeviceListEventCount);
            Assert.Equal("fixed", returnedEvent.EventTimingStrategy);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenAddEventIsCalledWithBadListId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), UnicodeEncoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1000/events", eventCreationData);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region RemoveEvent
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenRemoveEventIsCalledWithBadListId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            HttpResponseMessage response = await clientUnderTest.DeleteAsync("api/1/automation/transmissionlist/1000/events/1");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenRemoveEventIsCalledWithBadEventId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            HttpResponseMessage response = await clientUnderTest.DeleteAsync("api/1/automation/transmissionlist/1/events/1000");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenRemoveEventIsCalled_RemovesEventFromList()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), UnicodeEncoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string content = await response.Content.ReadAsStringAsync();
            TransmissionListEventDTO returnedEvent = JsonSerializer.Deserialize<TransmissionListEventDTO>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            response = await clientUnderTest.DeleteAsync($"api/1/automation/transmissionlist/1/events/{returnedEvent.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1/events");

            content = await response.Content.ReadAsStringAsync();
            List<TransmissionListEventDTO> returnedList = JsonSerializer.Deserialize<List<TransmissionListEventDTO>>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            Assert.Single(returnedList);
        }
        #endregion

        #region ClearList
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenClearListIsCalledWithBadListId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1000/clear", new StringContent(""));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenClearListIsCalled_RemovesAllEventsFromList()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), UnicodeEncoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/clear", new StringContent(""));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1/events");

            string content = await response.Content.ReadAsStringAsync();
            List<TransmissionListEventDTO> returnedList = JsonSerializer.Deserialize<List<TransmissionListEventDTO>>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            Assert.Empty(returnedList);
        }
        #endregion
        
        #region Play
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenPlayTransmissionListIsCalledWithBadListId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1000/play", new StringContent(""));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenPlayTransmissionListIsCalled_CreatesRelatedDeviceListEvents()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), UnicodeEncoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/play", new StringContent(""));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1/events");

            string content = await response.Content.ReadAsStringAsync();
            List<TransmissionListEventDTO> returnedList = JsonSerializer.Deserialize<List<TransmissionListEventDTO>>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            Assert.Equal(2, returnedList.Count);
            Assert.True(returnedList[0].RelatedDeviceListEventCount > 0);
            Assert.True(returnedList[1].RelatedDeviceListEventCount > 0);
        }
        #endregion

        #region Update Event
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenMoveEventIsCalled_ReturnsMovedEvent()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            
            string content = await response.Content.ReadAsStringAsync();
            TransmissionListEventDTO returnedEvent = JsonSerializer.Deserialize<TransmissionListEventDTO>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            int returnedId = returnedEvent.Id;

            _ = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            TransmissionListEventMoveDTO moveData = new TransmissionListEventMoveDTO() { PreviousPosition = 0, TargetPosition = 1};
            var eventUpdateData = new StringContent(JsonSerializer.Serialize(moveData), Encoding.UTF8, "application/json");

            response = await clientUnderTest.PatchAsync($"api/1/automation/transmissionlist/1/events/{returnedId}/move", eventUpdateData);
            content = await response.Content.ReadAsStringAsync();
            returnedEvent = JsonSerializer.Deserialize<TransmissionListEventDTO>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            Assert.Equal(returnedId, returnedEvent.Id);
            Assert.Equal("SCHEDULED", returnedEvent.EventState);
            Assert.Equal(1, returnedEvent.EventFeatureCount);
            Assert.Equal(1, returnedEvent.RelatedDeviceListEventCount);
            Assert.Equal("fixed", returnedEvent.EventTimingStrategy);
        }
        
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenMoveEventIsCalled_SuccessfullyMovesEvent()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            string content = await response.Content.ReadAsStringAsync();
            TransmissionListEventDTO returnedEvent = JsonSerializer.Deserialize<TransmissionListEventDTO>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            int returnedId = returnedEvent.Id;

            _ = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            TransmissionListEventMoveDTO moveData = new TransmissionListEventMoveDTO() { PreviousPosition = 0, TargetPosition = 1 };
            var eventUpdateData = new StringContent(JsonSerializer.Serialize(moveData), Encoding.UTF8, "application/json");

            response = await clientUnderTest.PatchAsync($"api/1/automation/transmissionlist/1/events/{returnedId}/move", eventUpdateData);
            content = await response.Content.ReadAsStringAsync();
            returnedEvent = JsonSerializer.Deserialize<TransmissionListEventDTO>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1/events");

            content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Content read as: {content}, Response status code: {response.StatusCode}");
            List<TransmissionListEventDTO> returnedList = JsonSerializer.Deserialize<List<TransmissionListEventDTO>>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            Assert.Equal(returnedList[1].Id, returnedEvent.Id);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenMoveEventIsCalledWithBadListId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            string content = await response.Content.ReadAsStringAsync();
            TransmissionListEventDTO returnedEvent = JsonSerializer.Deserialize<TransmissionListEventDTO>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            int returnedId = returnedEvent.Id;

            _ = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            TransmissionListEventMoveDTO moveData = new TransmissionListEventMoveDTO() { PreviousPosition = 0, TargetPosition = 1 };
            var eventUpdateData = new StringContent(JsonSerializer.Serialize(moveData), Encoding.UTF8, "application/json");
            response = await clientUnderTest.PatchAsync($"api/1/automation/transmissionlist/100/events/{returnedId}/move", eventUpdateData);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenMoveEventIsCalledWithBadEventId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), Encoding.UTF8, "application/json");

            await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            TransmissionListEventMoveDTO moveData = new TransmissionListEventMoveDTO() { PreviousPosition = 0, TargetPosition = 1 };
            var eventUpdateData = new StringContent(JsonSerializer.Serialize(moveData), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await clientUnderTest.PatchAsync($"api/1/automation/transmissionlist/1/events/100/move", eventUpdateData);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenMoveEventIsCalledWithIncorrectPreviousPosition_ReturnsBadRequest()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            string content = await response.Content.ReadAsStringAsync();
            TransmissionListEventDTO returnedEvent = JsonSerializer.Deserialize<TransmissionListEventDTO>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            int returnedId = returnedEvent.Id;

            await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            TransmissionListEventMoveDTO moveData = new TransmissionListEventMoveDTO() { PreviousPosition = 1, TargetPosition = 0 };
            var eventUpdateData = new StringContent(JsonSerializer.Serialize(moveData), Encoding.UTF8, "application/json");
            response = await clientUnderTest.PatchAsync($"api/1/automation/transmissionlist/1/events/{returnedId}/move", eventUpdateData);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenMoveEventIsCalledWithIncorrectTargetPosition_ReturnsBadRequest()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            string content = await response.Content.ReadAsStringAsync();
            TransmissionListEventDTO returnedEvent = JsonSerializer.Deserialize<TransmissionListEventDTO>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            int returnedId = returnedEvent.Id;

            await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            TransmissionListEventMoveDTO moveData = new TransmissionListEventMoveDTO() { PreviousPosition = 0, TargetPosition = 10 };
            var eventUpdateData = new StringContent(JsonSerializer.Serialize(moveData), Encoding.UTF8, "application/json");
            response = await clientUnderTest.PatchAsync($"api/1/automation/transmissionlist/1/events/{returnedId}/move", eventUpdateData);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion
    }
}
