using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CBS.Siren.Application;
using CBS.Siren.Data;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CBS.Siren.Test
{
    public class TransmissionListHandlerUnitTest
    {
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task GetListEvents_WithInvalidId_ThrowsException()
        {
            var logger = new Mock<ILogger<TransmissionListHandler>>();
            var dataLayer = new Mock<IDataLayer>();

            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, null),
                new TransmissionListEvent(null, null)
            });
            list.Id = "1";
            dataLayer.Setup(mock => mock.TransmissionLists()).ReturnsAsync(new List<TransmissionList>(){list});
            TransmissionListHandler codeUnderTest = new TransmissionListHandler(logger.Object, dataLayer.Object);

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.GetListEvents("30"));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task GetListEvents_WithValidId_ReturnsEventsFromList()
        {
            var logger = new Mock<ILogger<TransmissionListHandler>>();
            var dataLayer = new Mock<IDataLayer>();

            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, null),
                new TransmissionListEvent(null, null)
            });
            list.Id = "1";
            dataLayer.Setup(mock => mock.TransmissionLists()).ReturnsAsync(new List<TransmissionList>(){list});
            TransmissionListHandler codeUnderTest = new TransmissionListHandler(logger.Object, dataLayer.Object);

            IEnumerable<TransmissionListEvent> events = await codeUnderTest.GetListEvents("1");

            Assert.Equal(2, events.ToList().Count);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddEvents_WithInvalidId_ThrowsException()
        {
            Assert.False(true);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddEvents_WithValidId_ReturnsEvent()
        {
            Assert.False(true);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddEvents_WithInvalidInput_ReturnsThrowsException()
        {
            Assert.False(true);
        }
    }
}