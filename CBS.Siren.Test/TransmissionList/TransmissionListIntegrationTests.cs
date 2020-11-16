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
using CBS.Siren.Time;
using CBS.Siren.Utilities;

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
                    TargetStartTime = DateTimeOffset.Parse("2020-03-22 12:30:10").ToTimecodeString()
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
                        },
                        Duration = "00:00:30:00"
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
            List<TransmissionListDTO> returnedLists = content.DeserializeJson<List<TransmissionListDTO>>();

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
        public async Task Service_WhenTransmissionListRequestedById_ReturnsTransmissionList()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            _ = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            HttpResponseMessage response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1");

            string content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Content read as: {content}, Response status code: {response.StatusCode}");
            TransmissionListDetailDTO returnedList = content.DeserializeJson<TransmissionListDetailDTO>();

            Assert.NotNull(returnedList);
            Assert.Equal("Stopped", returnedList.ListState);
            Assert.Single(returnedList.Events);
            Assert.Null(returnedList.CurrentEventId);
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
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO), Encoding.UTF8, "application/json");

            _ = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            HttpResponseMessage response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1/events");

            string content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Content read as: {content}, Response status code: {response.StatusCode}");
            List<TransmissionListEventDTO> returnedList = content.DeserializeJson<List<TransmissionListEventDTO>>();

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

        #region GetListEventById
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenGetEventIsCalledWithBadListId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            HttpResponseMessage response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1000/events/1");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenGetEventIsCalledWithBadEventId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            HttpResponseMessage response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1/events/1000");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenGetEventIsCalledWithValidIds_ReturnsCreatedEvent()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            string content = await response.Content.ReadAsStringAsync();
            TransmissionListDTO createdEvent = content.DeserializeJson<TransmissionListDTO>();
            int createdEventId = createdEvent.Id;

            response = await clientUnderTest.GetAsync($"api/1/automation/transmissionlist/1/events/{createdEventId}");
            content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Content read as: {content}, Response status code: {response.StatusCode}");
            TransmissionListEventDetailDTO returnedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();

            Assert.Equal("SCHEDULED", returnedEvent.EventState);
            Assert.Single(returnedEvent.EventFeatures);
            Assert.NotNull(returnedEvent.EventFeatures[0].Uid);
            Assert.Equal("video", returnedEvent.EventFeatures[0].FeatureType);
            Assert.Equal("00:00:30:00", returnedEvent.EventFeatures[0].Duration);
            Assert.Equal("primaryVideo", returnedEvent.EventFeatures[0].PlayoutStrategy.StrategyType);
            Assert.Equal("mediaSource", returnedEvent.EventFeatures[0].SourceStrategy.StrategyType);
            Assert.Equal("00:00:00:00", returnedEvent.EventFeatures[0].SourceStrategy.SOM);
            Assert.Equal("00:00:30:00", returnedEvent.EventFeatures[0].SourceStrategy.EOM);
            Assert.Equal("TestInstance", returnedEvent.EventFeatures[0].SourceStrategy.MediaName);
            Assert.Equal("DemoDevice", returnedEvent.EventFeatures[0].Device.Name);
            Assert.Equal("00:00:05:00", returnedEvent.EventFeatures[0].Device.DeviceProperties.Preroll);
            Assert.Equal("STOPPED", returnedEvent.EventFeatures[0].Device.CurrentStatus);
            Assert.Equal(1, returnedEvent.RelatedDeviceListEventCount);
            Assert.Equal("fixed", returnedEvent.EventTimingStrategy.StrategyType);
            Assert.Equal("2020-03-22T12:30:10:00", returnedEvent.EventTimingStrategy.TargetStartTime);
            Assert.Equal("00:00:30:00", returnedEvent.ExpectedDuration);
            Assert.Equal("2020-03-22T12:30:10:00", returnedEvent.ExpectedStartTime);
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
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            string content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Content read as: {content}, Response status code: {response.StatusCode}");
            TransmissionListEventDetailDTO returnedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();

            Assert.Equal("SCHEDULED", returnedEvent.EventState);
            Assert.Single(returnedEvent.EventFeatures);
            Assert.Equal(1, returnedEvent.RelatedDeviceListEventCount);
            Assert.Equal("fixed", returnedEvent.EventTimingStrategy.StrategyType);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenAddEventIsCalledWithBadListId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1000/events", eventCreationData);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenAddEventIsCalled_CreatesEventFeatureWithUid()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            string content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Content read as: {content}, Response status code: {response.StatusCode}");
            TransmissionListEventDetailDTO returnedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();

            Assert.Equal("SCHEDULED", returnedEvent.EventState);
            Assert.Single(returnedEvent.EventFeatures);
            Assert.NotNull(returnedEvent.EventFeatures[0].Uid);
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
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string content = await response.Content.ReadAsStringAsync();
            TransmissionListEventDetailDTO returnedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();

            response = await clientUnderTest.DeleteAsync($"api/1/automation/transmissionlist/1/events/{returnedEvent.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1/events");

            content = await response.Content.ReadAsStringAsync();
            List<TransmissionListEventDTO> returnedList = content.DeserializeJson<List<TransmissionListEventDTO>>();

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
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/clear", new StringContent(""));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1/events");

            string content = await response.Content.ReadAsStringAsync();
            List<TransmissionListEventDTO> returnedList = content.DeserializeJson<List<TransmissionListEventDTO>>();

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
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/play", new StringContent(""));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1/events");

            string content = await response.Content.ReadAsStringAsync();
            List<TransmissionListEventDTO> returnedList = content.DeserializeJson<List<TransmissionListEventDTO>>();

            Assert.Equal(2, returnedList.Count);
            Assert.True(returnedList[0].RelatedDeviceListEventCount > 0);
            Assert.True(returnedList[1].RelatedDeviceListEventCount > 0);
        }
        #endregion
        
        #region Stop
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenStopTransmissionListIsCalledWithBadListId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1000/stop", new StringContent(""));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenStopTransmissionListIsCalled_UpdatesListStateAndClearsDeviceList()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/play", new StringContent(""));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            
            response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/stop", new StringContent(""));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1");

            string content = await response.Content.ReadAsStringAsync();
            TransmissionListDetailDTO returnedList = content.DeserializeJson<TransmissionListDetailDTO>();

            Assert.Equal("Stopped", returnedList.ListState);
            Assert.Equal(2, returnedList.Events.Count);
            Assert.True(returnedList.Events[0].RelatedDeviceListEventCount == 0);
            Assert.True(returnedList.Events[1].RelatedDeviceListEventCount == 0);


        }
        #endregion

        #region Next
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenNextTransmissionListIsCalledWithBadListId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1000/next", new StringContent(""));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenNextTransmissionListIsCalled_MovesPlayToTheSubsequentEvent()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/play", new StringContent(""));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/next", new StringContent(""));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1");

            string content = await response.Content.ReadAsStringAsync();
            TransmissionListDetailDTO returnedList = content.DeserializeJson<TransmissionListDetailDTO>();

            Assert.Equal(2, returnedList.Events.Count);
            Assert.Equal(returnedList.Events[1].Id, returnedList.CurrentEventId);
            Assert.True(returnedList.Events[0].EventState == "PLAYED");
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
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            
            string content = await response.Content.ReadAsStringAsync();
            TransmissionListEventDetailDTO returnedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();
            int returnedId = returnedEvent.Id;

            _ = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            TransmissionListEventMoveDTO moveData = new TransmissionListEventMoveDTO() { PreviousPosition = 0, TargetPosition = 1};
            var eventUpdateData = new StringContent(moveData.SerializeToJson(), Encoding.UTF8, "application/json");

            response = await clientUnderTest.PatchAsync($"api/1/automation/transmissionlist/1/events/{returnedId}/move", eventUpdateData);
            content = await response.Content.ReadAsStringAsync();
            returnedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();

            Assert.Equal(returnedId, returnedEvent.Id);
            Assert.Equal("SCHEDULED", returnedEvent.EventState);
            Assert.Single(returnedEvent.EventFeatures);
            Assert.Equal(1, returnedEvent.RelatedDeviceListEventCount);
            Assert.Equal("fixed", returnedEvent.EventTimingStrategy.StrategyType);
        }
        
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenMoveEventIsCalled_SuccessfullyMovesEvent()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            string content = await response.Content.ReadAsStringAsync();
            TransmissionListEventDetailDTO returnedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();
            int returnedId = returnedEvent.Id;

            _ = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            TransmissionListEventMoveDTO moveData = new TransmissionListEventMoveDTO() { PreviousPosition = 0, TargetPosition = 1 };
            var eventUpdateData = new StringContent(moveData.SerializeToJson(), Encoding.UTF8, "application/json");

            response = await clientUnderTest.PatchAsync($"api/1/automation/transmissionlist/1/events/{returnedId}/move", eventUpdateData);
            content = await response.Content.ReadAsStringAsync();
            returnedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();

            response = await clientUnderTest.GetAsync("api/1/automation/transmissionlist/1/events");
            content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Content read as: {content}, Response status code: {response.StatusCode}");
            List<TransmissionListEventDTO> returnedList = content.DeserializeJson<List<TransmissionListEventDTO>>();

            Assert.Equal(returnedList[1].Id, returnedEvent.Id);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenMoveEventIsCalledWithBadListId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            string content = await response.Content.ReadAsStringAsync();
            TransmissionListEventDetailDTO returnedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();
            int returnedId = returnedEvent.Id;

            _ = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            TransmissionListEventMoveDTO moveData = new TransmissionListEventMoveDTO() { PreviousPosition = 0, TargetPosition = 1 };
            var eventUpdateData = new StringContent(moveData.SerializeToJson(), Encoding.UTF8, "application/json");
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
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            TransmissionListEventMoveDTO moveData = new TransmissionListEventMoveDTO() { PreviousPosition = 0, TargetPosition = 1 };
            var eventUpdateData = new StringContent(moveData.SerializeToJson(), Encoding.UTF8, "application/json");
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
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            string content = await response.Content.ReadAsStringAsync();
            TransmissionListEventDetailDTO returnedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();
            int returnedId = returnedEvent.Id;

            await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            TransmissionListEventMoveDTO moveData = new TransmissionListEventMoveDTO() { PreviousPosition = 1, TargetPosition = 0 };
            var eventUpdateData = new StringContent(moveData.SerializeToJson(), Encoding.UTF8, "application/json");
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
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            string content = await response.Content.ReadAsStringAsync();
            TransmissionListEventDetailDTO returnedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();
            int returnedId = returnedEvent.Id;

            await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            TransmissionListEventMoveDTO moveData = new TransmissionListEventMoveDTO() { PreviousPosition = 0, TargetPosition = 10 };
            var eventUpdateData = new StringContent(moveData.SerializeToJson(), Encoding.UTF8, "application/json");
            response = await clientUnderTest.PatchAsync($"api/1/automation/transmissionlist/1/events/{returnedId}/move", eventUpdateData);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenUpdateEventIsCalled_SuccessfullyUpdatesEvent()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            string content = await response.Content.ReadAsStringAsync();
            TransmissionListEventDetailDTO returnedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();
            int returnedId = returnedEvent.Id;

            _ = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            DateTimeOffset expectedStartTime = DateTimeOffset.Parse("2020-05-22 17:30:36");
            TimeSpan expectedFeatureDuration = TimeSpan.FromSeconds(40);
            TransmissionListEventUpsertDTO updateDTO = GetListEventCreationDTO();
            updateDTO.TimingData.TargetStartTime = expectedStartTime.ToTimecodeString();
            updateDTO.Features[0].Duration = expectedFeatureDuration.ToTimecodeString();

            var eventUpdateData = new StringContent(updateDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            _ = await clientUnderTest.PutAsync($"api/1/automation/transmissionlist/1/events/{returnedId}", eventUpdateData);
            response = await clientUnderTest.GetAsync($"api/1/automation/transmissionlist/1/events/{returnedId}");

            content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Content read as: {content}, Response status code: {response.StatusCode}");
            TransmissionListEventDetailDTO updatedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();

            Assert.NotNull(updatedEvent);
            Assert.Equal(expectedStartTime, updatedEvent.ExpectedStartTime.ConvertTimecodeStringToDateTime());
            Assert.Single(updatedEvent.EventFeatures);
            Assert.Equal(expectedFeatureDuration, updatedEvent.EventFeatures[0].Duration.ConvertTimecodeStringToTimeSpan());
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenUpdateEventIsCalledWithBadListId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            string content = await response.Content.ReadAsStringAsync();
            TransmissionListEventDetailDTO returnedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();
            int returnedId = returnedEvent.Id;

            _ = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            DateTimeOffset expectedStartTime = DateTimeOffset.Parse("2020-05-22 17:30:36");
            TransmissionListEventUpsertDTO updateDTO = GetListEventCreationDTO();
            updateDTO.TimingData.TargetStartTime = expectedStartTime.ToTimecodeString();

            var eventUpdateData = new StringContent(updateDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            response = await clientUnderTest.PutAsync($"api/1/automation/transmissionlist/100/events/{returnedId}", eventUpdateData);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenUpdateEventIsCalledWithBadEventId_ReturnsNotFound()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            _ = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);
            _ = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            DateTimeOffset expectedStartTime = DateTimeOffset.Parse("2020-05-22 17:30:36");
            TransmissionListEventUpsertDTO updateDTO = GetListEventCreationDTO();
            updateDTO.TimingData.TargetStartTime = expectedStartTime.ToTimecodeString();

            var eventUpdateData = new StringContent(updateDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PutAsync($"api/1/automation/transmissionlist/1/events/100", eventUpdateData);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenUpdateEventIsCalled_MaintainsExistingFeatureUid()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            string content = await response.Content.ReadAsStringAsync();
            TransmissionListEventDetailDTO returnedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();
            int returnedId = returnedEvent.Id;
            string expectedUid = returnedEvent.EventFeatures[0].Uid;

            DateTimeOffset expectedStartTime = DateTimeOffset.Parse("2020-05-22 17:30:36");
            TransmissionListEventUpsertDTO updateDTO = GetListEventCreationDTO();
            updateDTO.Features[0].Uid = expectedUid;
            updateDTO.TimingData.TargetStartTime = expectedStartTime.ToTimecodeString();

            var eventUpdateData = new StringContent(updateDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            _ = await clientUnderTest.PutAsync($"api/1/automation/transmissionlist/1/events/{returnedId}", eventUpdateData);
            response = await clientUnderTest.GetAsync($"api/1/automation/transmissionlist/1/events/{returnedId}");

            content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Content read as: {content}, Response status code: {response.StatusCode}");
            TransmissionListEventDetailDTO updatedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();

            Assert.NotNull(updatedEvent);
            Assert.Single(updatedEvent.EventFeatures);
            Assert.Equal(expectedUid, updatedEvent.EventFeatures[0].Uid);
        }
        
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenUpdateEventIsCalledWithoutFeatureUid_GeneratesANewFeatureUid()
        {
            using WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            using HttpClient clientUnderTest = factory.CreateClient();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            var eventCreationData = new StringContent(creationDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/automation/transmissionlist/1/events", eventCreationData);

            string content = await response.Content.ReadAsStringAsync();
            TransmissionListEventDetailDTO returnedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();
            int returnedId = returnedEvent.Id;
            string originalUid = returnedEvent.EventFeatures[0].Uid;

            DateTimeOffset expectedStartTime = DateTimeOffset.Parse("2020-05-22 17:30:36");
            TransmissionListEventUpsertDTO updateDTO = GetListEventCreationDTO();
            updateDTO.TimingData.TargetStartTime = expectedStartTime.ToTimecodeString();

            var eventUpdateData = new StringContent(updateDTO.SerializeToJson(), Encoding.UTF8, "application/json");

            _ = await clientUnderTest.PutAsync($"api/1/automation/transmissionlist/1/events/{returnedId}", eventUpdateData);
            response = await clientUnderTest.GetAsync($"api/1/automation/transmissionlist/1/events/{returnedId}");

            content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Content read as: {content}, Response status code: {response.StatusCode}");
            TransmissionListEventDetailDTO updatedEvent = content.DeserializeJson<TransmissionListEventDetailDTO>();

            Assert.NotNull(updatedEvent);
            Assert.Single(updatedEvent.EventFeatures);
            Assert.NotNull(updatedEvent.EventFeatures[0].Uid);
            Assert.NotEqual(originalUid, updatedEvent.EventFeatures[0].Uid);
        }
        #endregion
    }
}
