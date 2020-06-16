using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CBS.Siren.Application;
using CBS.Siren.Data;
using CBS.Siren.DTO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CBS.Siren.Test
{
    public class TransmissionListHandlerUnitTest
    {
        private TransmissionListEventCreationDTO GetListEventCreationDTO()
        {
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

            return creationDTO;
        }
        
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
            var logger = new Mock<ILogger<TransmissionListHandler>>();
            var dataLayer = new Mock<IDataLayer>();

            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, null),
                new TransmissionListEvent(null, null)
            });
            list.Id = "1";
            dataLayer.Setup(mock => mock.TransmissionLists()).ReturnsAsync(new List<TransmissionList>(){list});
            TransmissionListHandler codeUnderTest = new TransmissionListHandler(logger.Object, dataLayer.Object);

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.AddEvent("30", new TransmissionListEventCreationDTO()));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddEvents_WithValidId_ReturnsEvent()
        {
            var logger = new Mock<ILogger<TransmissionListHandler>>();
            var dataLayer = new Mock<IDataLayer>();
            dataLayer.Setup(mock => mock.MediaInstances()).ReturnsAsync(new List<MediaInstance>(){new MediaInstance("TestInstance", new TimeSpan(0,0,30))});

            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, null),
                new TransmissionListEvent(null, null)
            });
            list.Id = "1";
            dataLayer.Setup(mock => mock.TransmissionLists()).ReturnsAsync(new List<TransmissionList>(){list});
            TransmissionListHandler codeUnderTest = new TransmissionListHandler(logger.Object, dataLayer.Object);

            TransmissionListEventCreationDTO creationDTO = GetListEventCreationDTO();
            
            TransmissionListEvent createdEvent = await codeUnderTest.AddEvent("1", creationDTO);

            Assert.Equal(TransmissionListEventState.Status.UNSCHEDULED, createdEvent.EventState.CurrentStatus);
            Assert.Single(createdEvent.EventFeatures);
            Assert.Empty(createdEvent.RelatedDeviceListEvents);
            Assert.Equal("fixed", createdEvent.EventTimingStrategy.StrategyType);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddEvents_WithValidId_SavesList()
        {
            var logger = new Mock<ILogger<TransmissionListHandler>>();
            var dataLayer = new Mock<IDataLayer>();

            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, null),
                new TransmissionListEvent(null, null)
            });
            list.Id = "1";
            dataLayer.Setup(mock => mock.TransmissionLists()).ReturnsAsync(new List<TransmissionList>(){list});
            TransmissionList savedList = null;
            dataLayer.Setup(mock => mock.AddUpdateTransmissionLists(It.IsAny<TransmissionList[]>())).Callback((TransmissionList[] lists) => { savedList = lists[0]; });
            TransmissionListHandler codeUnderTest = new TransmissionListHandler(logger.Object, dataLayer.Object);

            TransmissionListEventCreationDTO creationDTO = GetListEventCreationDTO();
            
            TransmissionListEvent addedEvent = await codeUnderTest.AddEvent("1", creationDTO);
            Assert.Equal(3, savedList.Events.Count);
            Assert.Equal(addedEvent.Id, savedList.Events[2].Id);

        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddEvents_WithInvalidInput_ReturnsThrowsException()
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

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.AddEvent("1", new TransmissionListEventCreationDTO()));
        }
    }
}