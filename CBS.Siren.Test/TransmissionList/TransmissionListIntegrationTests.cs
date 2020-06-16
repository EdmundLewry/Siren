using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using CBS.Siren.DTO;
using System.Linq;
using CBS.Siren.Controllers;
using System;

namespace CBS.Siren.Test
{
    public class TransmissionListIntegrationTests
    {
        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenTransmissionListsRequested_ReturnsCollectionOfTransmissionLists()
        {
            WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            HttpClient clientUnderTest = factory.CreateClient();
            
            HttpResponseMessage response = await clientUnderTest.GetAsync("api/1/transmissionlist");
            
            string content = await response.Content.ReadAsStringAsync();
            List<TransmissionListDTO> returnedLists = JsonSerializer.Deserialize<List<TransmissionListDTO>>(content, new JsonSerializerOptions(){PropertyNameCaseInsensitive = true});

            Assert.Single(returnedLists);
            //Attempt to parse id to ensure it's valid
            int.Parse(returnedLists.First().Id);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenTransmissionListEventsRequested_ReturnsCollectionOfTransmissionListEvents()
        {
            WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            HttpClient clientUnderTest = factory.CreateClient();
            
            HttpResponseMessage response = await clientUnderTest.GetAsync("api/1/transmissionlist/0/events");
            
            string content = await response.Content.ReadAsStringAsync();
            List<TransmissionListEventDTO> returnedList = JsonSerializer.Deserialize<List<TransmissionListEventDTO>>(content, new JsonSerializerOptions(){PropertyNameCaseInsensitive = true});

            //Should use API to add event later
            //Assert.NotEmpty(returnedList);
        }

        [Fact]
        [Trait("TestType", "IntegrationTest")]
        public async Task Service_WhenAddEventIsCalled_ReturnsCreatedEvent()
        {
            WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            HttpClient clientUnderTest = factory.CreateClient();
            
            TransmissionListEventCreationDTO creationDTO = new TransmissionListEventCreationDTO(){
                TimingData = new TimingStrategyCreationDTO(){
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
            var eventCreationData = new StringContent(JsonSerializer.Serialize(creationDTO));

            HttpResponseMessage response = await clientUnderTest.PostAsync("api/1/transmissionlist/0/events", eventCreationData);
            
            string content = await response.Content.ReadAsStringAsync();
            TransmissionListEventDTO returnedEvent = JsonSerializer.Deserialize<TransmissionListEventDTO>(content, new JsonSerializerOptions(){PropertyNameCaseInsensitive = true});
            
            Assert.Equal("UNSCHEDULED", returnedEvent.EventState);
            Assert.Equal(1, returnedEvent.EventFeatureCount);
            Assert.Equal(0, returnedEvent.RelatedDeviceListEventCount);
            Assert.Equal("fixed", returnedEvent.EventTimingStrategy);
        }
    }
}
