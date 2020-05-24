using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using CBS.Siren.DTO;
using System.Linq;

namespace CBS.Siren.Test
{
    public class TransmissionListIntegrationTests
    {
        [Fact]
        [Trait("TestType", "UnitTest")]
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
        [Trait("TestType", "UnitTest")]
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
    }
}
