using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;

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
            List<TransmissionList> returnedLists = JsonSerializer.Deserialize<List<TransmissionList>>(content);

            Assert.Single(returnedLists);
        }
    }
}
