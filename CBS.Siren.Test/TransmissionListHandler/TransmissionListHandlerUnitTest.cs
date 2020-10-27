using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CBS.Siren.Application;
using CBS.Siren.Data;
using CBS.Siren.Device;
using CBS.Siren.DTO;
using CBS.Siren.Time;
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
                new TransmissionListEvent(new FixedStartEventTimingStrategy(DateTimeOffset.UtcNow), 
                                          new List<IEventFeature>(){ 
                                            new VideoPlaylistEventFeature(new PrimaryVideoPlayoutStrategy(), 
                                                                          new MediaSourceStrategy(new MediaInstance("Test", TimeSpan.FromSeconds(30)), TimeSpan.Zero, TimeSpan.FromSeconds(30)), TimeSpan.FromSeconds(30))
                                          }){ 
                    Id = 1 
                },
                new TransmissionListEvent(null, null){ 
                    Id = 2 
                }
            })
            {
                Id = 1
            };
        }
        private TransmissionListEventUpsertDTO GetListEventCreationDTO()
        {
            TransmissionListEventUpsertDTO creationDTO = new TransmissionListEventUpsertDTO()
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

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task GetListById_WithInvalidId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.GetListById(30));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task GetListById_WithValidId_ReturnsList()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            TransmissionList transmissionList = await codeUnderTest.GetListById(1);

            Assert.Equal(2, transmissionList.Events.ToList().Count);
            Assert.Equal(TransmissionListState.Stopped, transmissionList.State);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task GetListEventById_WithInvalidListId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.GetListEventById(30, 1));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task GetListEventById_WithInvalidEventId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.GetListEventById(1, 30));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task GetListEventById_WithValidEventId_ReturnsEvent()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            TransmissionListEvent transmissionListEvent = await codeUnderTest.GetListEventById(1, 1);

            Assert.NotNull(transmissionListEvent);
            Assert.Equal(1, transmissionListEvent.Id);
        }
        #endregion

        #region Add
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddEvents_WithInvalidId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.AddEvent(30, new TransmissionListEventUpsertDTO()));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddEvents_WithValidId_ReturnsEvent()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
            
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

            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();            
            TransmissionListEvent addedEvent = await codeUnderTest.AddEvent(1, creationDTO);

            Assert.Equal(3, savedList.Events.Count);
            Assert.Equal(addedEvent.Id, savedList.Events[2].Id);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddEvent_WithInvalidInput_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.AddEvent(1, new TransmissionListEventUpsertDTO()));
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
            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
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
            TransmissionListEventUpsertDTO creationDTO = GetListEventCreationDTO();
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

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.RemoveEvent(1, 3000));
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

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task ClearList_WithValidInput_ShouldCallIntoListService()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await codeUnderTest.ClearList(1);

            _listService.Verify((service) => service.OnTransmissionListChanged(0));
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

        #region Update
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task ChangeEventPosition_WithInvalidListId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.ChangeEventPosition(30, 0, 0, 1));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task ChangeEventPosition_WithInvalidEventId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.ChangeEventPosition(1, 30, 0, 1));
        }
        
        [Theory]
        [Trait("TestType", "UnitTest")]
        [InlineData(100, 0)]
        [InlineData(0, 100)]
        public async Task ChangeEventPosition_WithInvalidPositions_ThrowsException(int previousIndex, int targetIndex)
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.ChangeEventPosition(1, 0, previousIndex, targetIndex));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task ChangeEventPosition_WithValidPositions_UpdatesPositionInList()
        {
            TransmissionList savedList = null;
            _dataLayer.Setup(mock => mock.AddUpdateTransmissionLists(It.IsAny<TransmissionList[]>())).Callback((TransmissionList[] lists) => { savedList = lists[0]; });

            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            TransmissionListEvent createdEvent = await codeUnderTest.ChangeEventPosition(1, _transmissionList.Events[0].Id, 0, 1);

            Assert.Equal(2, savedList.Events.Count);
            Assert.Equal(createdEvent.Id, savedList.Events[1].Id);
        }
        
        [Theory]
        [Trait("TestType", "UnitTest")]
        [InlineData(0, 1, 0)]
        [InlineData(1, 0, 0)]
        public async Task ChangeEventPosition_WithValidPositions_CallsIntoListService(int from, int to, int expected)
        {
            TransmissionList savedList = null;
            _dataLayer.Setup(mock => mock.AddUpdateTransmissionLists(It.IsAny<TransmissionList[]>())).Callback((TransmissionList[] lists) => { savedList = lists[0]; });

            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await codeUnderTest.ChangeEventPosition(1, _transmissionList.Events[from].Id, from, to);

            _listService.Verify((service) => service.OnTransmissionListChanged(expected));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task ChangeEventPosition_WithValidPositions_ReturnsEvent()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            int eventId = _transmissionList.Events[0].Id;
            TransmissionListEvent returnedEvent = await codeUnderTest.ChangeEventPosition(1, eventId, 0, 1);

            Assert.Equal(eventId, returnedEvent.Id);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task ChangeEventPosition_WithValidPositions_SavesTheList()
        {
            TransmissionList savedList = null;
            _dataLayer.Setup(mock => mock.AddUpdateTransmissionLists(It.IsAny<TransmissionList[]>())).Callback((TransmissionList[] lists) => { savedList = lists[0]; });

            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            await codeUnderTest.ChangeEventPosition(1, _transmissionList.Events[0].Id, 0, 1);

            _dataLayer.Verify(mock => mock.AddUpdateTransmissionLists(It.IsAny<TransmissionList[]>()), Times.Once);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task UpdateEvent_WithInvalidListId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            int eventId = _transmissionList.Events[0].Id;

            TransmissionListEventUpsertDTO listEventUpdateDTO = new TransmissionListEventUpsertDTO();
            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.UpdateEventDetails(30, eventId, listEventUpdateDTO));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task UpdateEvent_WithInvalidEventId_ThrowsException()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            int eventId = 30;
            TransmissionListEventUpsertDTO listEventUpdateDTO = new TransmissionListEventUpsertDTO();
            await Assert.ThrowsAnyAsync<Exception>(() => codeUnderTest.UpdateEventDetails(_transmissionList.Id, eventId, listEventUpdateDTO));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task UpdateEvent_WithValidIds_ReturnsEvent()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            int eventId = _transmissionList.Events[0].Id;
            TransmissionListEventUpsertDTO listEventUpdateDTO = GetListEventCreationDTO();
            TransmissionListEvent returnedEvent = await codeUnderTest.UpdateEventDetails(_transmissionList.Id, eventId, listEventUpdateDTO);

            Assert.Equal(eventId, returnedEvent.Id);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task UpdateEvent_WithValidIds_UpdatesEventDetails()
        {
            TransmissionList savedList = null;
            _dataLayer.Setup(mock => mock.AddUpdateTransmissionLists(It.IsAny<TransmissionList[]>())).Callback((TransmissionList[] lists) => { savedList = lists[0]; });
            
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            DateTimeOffset newStartTime = DateTimeOffset.Parse("12/02/2020 12:00:00");
            TimeSpan newDuration = TimeSpan.FromSeconds(40);

            int eventId = _transmissionList.Events[0].Id;
            TransmissionListEventUpsertDTO listEventUpdateDTO = GetListEventCreationDTO();
            listEventUpdateDTO.TimingData.TargetStartTime = newStartTime.ToTimecodeString();
            listEventUpdateDTO.Features[0].Duration = newDuration.ToTimecodeString();
            TransmissionListEvent returnedEvent = await codeUnderTest.UpdateEventDetails(_transmissionList.Id, eventId, listEventUpdateDTO);

            Assert.Equal(eventId, returnedEvent.Id);

            Assert.Equal(TransmissionListEventState.Status.UNSCHEDULED, returnedEvent.EventState.CurrentStatus);
            Assert.Single(returnedEvent.EventFeatures);
            Assert.Null(returnedEvent.EventFeatures[0].DeviceListEventId);
            Assert.Equal(newDuration, returnedEvent.EventFeatures[0].Duration);
            Assert.Equal("fixed", returnedEvent.EventTimingStrategy.StrategyType);
            Assert.Equal(newStartTime, returnedEvent.EventTimingStrategy.CalculateStartTime(eventId, _transmissionList));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task UpdateEvent_WithValidIds_TriggersListChange()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            int eventId = _transmissionList.Events[0].Id;
            TransmissionListEventUpsertDTO listEventUpdateDTO = GetListEventCreationDTO();

            await codeUnderTest.UpdateEventDetails(_transmissionList.Id, eventId, listEventUpdateDTO);
            _listService.Verify((service) => service.OnTransmissionListChanged(0));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task UpdateEvent_WithValidIds_UpdatesDataLayer()
        {
            TransmissionListHandler codeUnderTest = CreateHandlerUnderTest();

            int eventId = _transmissionList.Events[0].Id;
            TransmissionListEventUpsertDTO listEventUpdateDTO = GetListEventCreationDTO();
            await codeUnderTest.UpdateEventDetails(_transmissionList.Id, eventId, listEventUpdateDTO);

            _dataLayer.Verify(mock => mock.AddUpdateTransmissionLists(It.IsAny<TransmissionList[]>()), Times.AtLeastOnce());
        }
        #endregion
    }
}