using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CBS.Siren.Application;
using CBS.Siren.Data;
using CBS.Siren.Device;
using CBS.Siren.DTO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CBS.Siren.Test
{
    public class TransmissionListHandlerUnitTest
    {
        private readonly Mock<ILogger<TransmissionListHandler>> _logger;
        private readonly Mock<ITransmissionListService> _listService;
        private readonly Mock<IDeviceManager> _deviceManager;
        private readonly Mock<IDataLayer> _dataLayer;
        private readonly TransmissionList _transmissionList;

        public TransmissionListHandlerUnitTest()
        {
            _logger = new Mock<ILogger<TransmissionListHandler>>();
            _listService = new Mock<ITransmissionListService>();
            _deviceManager = new Mock<IDeviceManager>();
            _dataLayer = new Mock<IDataLayer>();
            
            _transmissionList = new TransmissionList(new List<TransmissionListEvent>(){
                new TransmissionListEvent(null, null),
                new TransmissionListEvent(null, null)
            })
            {
                Id = 1
            };
        }
        private TransmissionListEventCreationDTO GetListEventCreationDTO()
        {
            TransmissionListEventCreationDTO creationDTO = new TransmissionListEventCreationDTO()
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
                            SOM = "00:00:00:00",
                            EOM = "00:00:30:00",
                            MediaName = "TestInstance"
                        }
                    }
                }
            };

            return creationDTO;
        }

        private TransmissionListHandler CreateHandlerUnderTest()
        {
            _dataLayer.Setup(mock => mock.TransmissionLists()).ReturnsAsync(new List<TransmissionList>() { _transmissionList });
            _dataLayer.Setup(mock => mock.MediaInstances()).ReturnsAsync(new List<MediaInstance>(){new MediaInstance("TestInstance", new TimeSpan(0,0,30))});

            Mock<ITransmissionListServiceStore> mockStore = new Mock<ITransmissionListServiceStore>();
            mockStore.Setup(mock => mock.GetTransmissionListServiceByListId(It.IsAny<int>())).Returns(_listService.Object);

            return new TransmissionListHandler(_logger.Object, _dataLayer.Object, mockStore.Object, _deviceManager.Object);
        }

        #region Get      
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task GetListEvents_WithInvalidId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.GetListEvents(30));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task GetListEvents_WithValidId_ReturnsEventsFromList()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            IEnumerable<TransmissionListEvent> events = await codeUnderTest.GetListEvents(1);

            Assert.Equal(2, events.ToList().Count);
        }
        #endregion

        #region Add
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddEvents_WithInvalidId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.AddEvent(30, new TransmissionListEventCreationDTO()));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddEvents_WithValidId_ReturnsEvent()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            TransmissionListEventCreationDTO creationDTO = GetListEventCreationDTO();
            
            TransmissionListEvent createdEvent = await codeUnderTest.AddEvent(1, creationDTO);

            Assert.Equal(TransmissionListEventState.Status.UNSCHEDULED, createdEvent.EventState.CurrentStatus);
            Assert.Single(createdEvent.EventFeatures);
            Assert.Null(createdEvent.EventFeatures[0].DeviceListEventId);
            Assert.Equal("fixed", createdEvent.EventTimingStrategy.StrategyType);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddEvents_WithValidId_SavesList()
        {
            TransmissionList savedList = null;
            _dataLayer.Setup(mock => mock.AddUpdateTransmissionLists(It.IsAny<TransmissionList[]>())).Callback((TransmissionList[] lists) => { savedList = lists[0]; });
            
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            TransmissionListEventCreationDTO creationDTO = GetListEventCreationDTO();            
            TransmissionListEvent addedEvent = await codeUnderTest.AddEvent(1, creationDTO);

            Assert.Equal(3, savedList.Events.Count);
            Assert.Equal(addedEvent.Id, savedList.Events[2].Id);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddEvent_WithInvalidInput_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.AddEvent(1, new TransmissionListEventCreationDTO()));
        }
        
        [Theory]
        [Trait("TestType", "UnitTest")]
        [InlineData(RelativePosition.Above, 0)]
        [InlineData(RelativePosition.Below, 1)]
        public async Task AddEvent_WithPositionData_InsertsEventInTheCorrectPosition(RelativePosition position, int expectedIndex)
        {
            TransmissionList savedList = null;
            _dataLayer.Setup(mock => mock.AddUpdateTransmissionLists(It.IsAny<TransmissionList[]>())).Callback((TransmissionList[] lists) => { savedList = lists[0]; });

            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();
            TransmissionListEventCreationDTO creationDTO = GetListEventCreationDTO();
            creationDTO.ListPosition = new ListPositionDTO() {
                RelativePosition = position,
                AssociatedEventId = _transmissionList.Events[0].Id
            };

            TransmissionListEvent createdEvent = await codeUnderTest.AddEvent(1, creationDTO);

            Assert.Equal(3, savedList.Events.Count);
            Assert.Equal(createdEvent.Id, savedList.Events[expectedIndex].Id);
        }

        [Theory]
        [Trait("TestType", "UnitTest")]
        [InlineData(RelativePosition.Above, 1)]
        [InlineData(RelativePosition.Below, 2)]
        public async Task AddEvent_WithPositionData_ShouldCallIntoListService(RelativePosition position, int expectedIndex)
        {
            TransmissionList savedList = null;
            _dataLayer.Setup(mock => mock.AddUpdateTransmissionLists(It.IsAny<TransmissionList[]>())).Callback((TransmissionList[] lists) => { savedList = lists[0]; });

            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();
            TransmissionListEventCreationDTO creationDTO = GetListEventCreationDTO();
            creationDTO.ListPosition = new ListPositionDTO()
            {
                RelativePosition = position,
                AssociatedEventId = _transmissionList.Events[1].Id
            };

            await codeUnderTest.AddEvent(1, creationDTO);

            _listService.Verify((service) => service.OnTransmissionListChanged(expectedIndex));
        }

        #endregion

        #region Remove
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task RemoveEvent_WithInvalidListId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.RemoveEvent(30, _transmissionList.Events[0].Id));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task RemoveEvent_WithInvalidEventId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.RemoveEvent(1, 30));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task RemoveEvent_WithValidInput_RemovesEventFromList()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            int eventId = _transmissionList.Events[0].Id;
            await codeUnderTest.RemoveEvent(1, eventId);

            Assert.Single(_transmissionList.Events);
            Assert.Null(_transmissionList.Events.FirstOrDefault(listEvent => listEvent.Id == eventId));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task RemoveEvent_WithValidInput_UpdatesDataLayer()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            int eventId = _transmissionList.Events[0].Id;
            await codeUnderTest.RemoveEvent(1, eventId);

            _dataLayer.Verify(mock => mock.AddUpdateTransmissionLists(It.IsAny<TransmissionList[]>()), Times.AtLeastOnce());
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task RemoveEvent_WithValidInput_ShouldCallIntoListService()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            int eventId = _transmissionList.Events[1].Id;
            await codeUnderTest.RemoveEvent(1, eventId);

            _listService.Verify((service) => service.OnTransmissionListChanged(1));
        }

        #endregion

        #region Clear
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task ClearList_WithInvalidListId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.ClearList(30));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task ClearList_WithValidInput_RemovesAllEventsFromList()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await codeUnderTest.ClearList(1);

            Assert.Empty(_transmissionList.Events);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task ClearList_WithValidInput_UpdatesDataLayer()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await codeUnderTest.ClearList(1);

            _dataLayer.Verify(mock => mock.AddUpdateTransmissionLists(It.IsAny<TransmissionList[]>()), Times.AtLeastOnce());
        }

        #endregion

        #region Play
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task PlayTransmisionList_WithInvalidListId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.PlayTransmissionList(30));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task PlayTransmissionList_WithValidInput_UpdatesDataLayer()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await codeUnderTest.PlayTransmissionList(1);

            _dataLayer.Verify(mock => mock.AddUpdateTransmissionLists(It.IsAny<TransmissionList[]>()), Times.AtLeastOnce());
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task PlayTransmisionList_WithValidId_InvokesTransmissionListServicePlay()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await codeUnderTest.PlayTransmissionList(1);

            _listService.Verify(mock => mock.PlayTransmissionList(), Times.Once);
        }
        #endregion
    }
}