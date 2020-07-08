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

        private TransmissionListHandler CreateHandlerUnderTest(Mock<IDataLayer> dataLayer = null)
        {
            var logger = new Mock<ILogger<TransmissionListHandler>>();
            var listService = new Mock<ITransmissionListService>();

            if(dataLayer == null)
            {
                dataLayer = new Mock<IDataLayer>();
            }

            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, null),
                new TransmissionListEvent(null, null)
            });
            list.Id = "1";
            dataLayer.Setup(mock => mock.TransmissionLists()).ReturnsAsync(new List<TransmissionList>() { list });
            return new TransmissionListHandler(logger.Object, dataLayer.Object, listService.Object);
        }

        #region GET        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task GetListEvents_WithInvalidId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.GetListEvents("30"));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task GetListEvents_WithValidId_ReturnsEventsFromList()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            IEnumerable<TransmissionListEvent> events = await codeUnderTest.GetListEvents("1");

            Assert.Equal(2, events.ToList().Count);
        }
        #endregion

        #region Add
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddEvents_WithInvalidId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.AddEvent("30", new TransmissionListEventCreationDTO()));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddEvents_WithValidId_ReturnsEvent()
        {
            var dataLayer = new Mock<IDataLayer>();
            dataLayer.Setup(mock => mock.MediaInstances()).ReturnsAsync(new List<MediaInstance>(){new MediaInstance("TestInstance", new TimeSpan(0,0,30))});

            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest(dataLayer);

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
            var listService = new Mock<ITransmissionListService>();

            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, null),
                new TransmissionListEvent(null, null)
            });
            list.Id = "1";
            dataLayer.Setup(mock => mock.TransmissionLists()).ReturnsAsync(new List<TransmissionList>(){list});
            TransmissionList savedList = null;
            dataLayer.Setup(mock => mock.AddUpdateTransmissionLists(It.IsAny<TransmissionList[]>())).Callback((TransmissionList[] lists) => { savedList = lists[0]; });
            TransmissionListHandler codeUnderTest = new TransmissionListHandler(logger.Object, dataLayer.Object, listService.Object);

            TransmissionListEventCreationDTO creationDTO = GetListEventCreationDTO();
            
            TransmissionListEvent addedEvent = await codeUnderTest.AddEvent("1", creationDTO);
            Assert.Equal(3, savedList.Events.Count);
            Assert.Equal(addedEvent.Id, savedList.Events[2].Id);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddEvent_WithInvalidInput_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.AddEvent("1", new TransmissionListEventCreationDTO()));
        }

        #endregion

        #region Remove
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task RemoveEvent_WithInvalidListId_ThrowsException()
        {
            var logger = new Mock<ILogger<TransmissionListHandler>>();
            var dataLayer = new Mock<IDataLayer>();
            var listService = new Mock<ITransmissionListService>();

            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, null)
            })
            {
                Id = "1"
            };
            dataLayer.Setup(mock => mock.TransmissionLists()).ReturnsAsync(new List<TransmissionList>() { list });
            TransmissionListHandler codeUnderTest = new TransmissionListHandler(logger.Object, dataLayer.Object, listService.Object);

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.RemoveEvent("30", list.Events[0].Id.ToString()));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task RemoveEvent_WithInvalidEventId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.RemoveEvent("1", "30"));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task RemoveEvent_WithValidInput_RemovesEventFromList()
        {
            var logger = new Mock<ILogger<TransmissionListHandler>>();
            var dataLayer = new Mock<IDataLayer>();
            var listService = new Mock<ITransmissionListService>();

            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, null),
                new TransmissionListEvent(null, null)
            })
            {
                Id = "1"
            };
            dataLayer.Setup(mock => mock.TransmissionLists()).ReturnsAsync(new List<TransmissionList>() { list });
            TransmissionListHandler codeUnderTest = new TransmissionListHandler(logger.Object, dataLayer.Object, listService.Object);

            string eventId = list.Events[0].Id.ToString();
            await codeUnderTest.RemoveEvent("1", eventId);

            Assert.Single(list.Events);
            Assert.Null(list.Events.FirstOrDefault(listEvent => listEvent.Id.ToString() == eventId));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task RemoveEvent_WithValidInput_UpdatesDataLayer()
        {
            var logger = new Mock<ILogger<TransmissionListHandler>>();
            var dataLayer = new Mock<IDataLayer>();
            var listService = new Mock<ITransmissionListService>();

            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, null)
            })
            {
                Id = "1"
            };
            dataLayer.Setup(mock => mock.TransmissionLists()).ReturnsAsync(new List<TransmissionList>() { list });

            TransmissionListHandler codeUnderTest = new TransmissionListHandler(logger.Object, dataLayer.Object, listService.Object);

            string eventId = list.Events[0].Id.ToString();
            await codeUnderTest.RemoveEvent("1", eventId);

            //Account for one extra call while we initialize the handler with an extra list during dev
            dataLayer.Verify(mock => mock.AddUpdateTransmissionLists(It.IsAny<TransmissionList[]>()), Times.AtLeast(2));
        }

        #endregion

        #region Clear
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task ClearList_WithInvalidListId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.ClearList("30"));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task ClearList_WithValidInput_RemovesAllEventsFromList()
        {
            var logger = new Mock<ILogger<TransmissionListHandler>>();
            var dataLayer = new Mock<IDataLayer>();
            var listService = new Mock<ITransmissionListService>();

            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, null),
                new TransmissionListEvent(null, null)
            })
            {
                Id = "1"
            };
            dataLayer.Setup(mock => mock.TransmissionLists()).ReturnsAsync(new List<TransmissionList>() { list });
            TransmissionListHandler codeUnderTest = new TransmissionListHandler(logger.Object, dataLayer.Object, listService.Object);

            await codeUnderTest.ClearList("1");

            Assert.Empty(list.Events);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task ClearList_WithValidInput_UpdatesDataLayer()
        {
            var logger = new Mock<ILogger<TransmissionListHandler>>();
            var dataLayer = new Mock<IDataLayer>();
            var listService = new Mock<ITransmissionListService>();

            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, null)
            })
            {
                Id = "1"
            };
            dataLayer.Setup(mock => mock.TransmissionLists()).ReturnsAsync(new List<TransmissionList>() { list });

            TransmissionListHandler codeUnderTest = new TransmissionListHandler(logger.Object, dataLayer.Object, listService.Object);

            await codeUnderTest.ClearList("1");

            //Account for one extra call while we initialize the handler with an extra list during dev
            dataLayer.Verify(mock => mock.AddUpdateTransmissionLists(It.IsAny<TransmissionList[]>()), Times.AtLeast(2));
        }

        #endregion

        #region Play
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task PlayTransmisionList_WithInvalidListId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.PlayTransmissionList("30"));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task PlayTransmisionList_WithValidId_InvokesTransmissionListServicePlay()
        {
            //Should clean these up
            var logger = new Mock<ILogger<TransmissionListHandler>>();
            var dataLayer = new Mock<IDataLayer>();
            var listService = new Mock<ITransmissionListService>();

            TransmissionList list = new TransmissionList(new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, null)
            })
            {
                Id = "1"
            };
            dataLayer.Setup(mock => mock.TransmissionLists()).ReturnsAsync(new List<TransmissionList>() { list });

            TransmissionListHandler codeUnderTest = new TransmissionListHandler(logger.Object, dataLayer.Object, listService.Object);

            await codeUnderTest.PlayTransmissionList("1");

            listService.Verify(mock => mock.PlayTransmissionList(), Times.Once);
        }
        #endregion
    }
}