using System.Collections.Generic;
using System.Threading.Tasks;
using CBS.Siren.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using AutoMapper;
using CBS.Siren.DTO;
using System;
using System.Text.Json;

namespace CBS.Siren.Controllers
{
    [ApiController]
    [Route("api/1/automation/transmissionlist")]
    public class TransmissionListAPIController : ControllerBase
    {
        private readonly ITransmissionListHandler _handler;
        private readonly IMapper _mapper;

        public ILogger<TransmissionListAPIController> Logger { get; private set; }

        public TransmissionListAPIController(ILogger<TransmissionListAPIController> logger, ITransmissionListHandler handler, IMapper mapper)
        {
            Logger = logger;
            _handler = handler;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransmissionListDTO>>> GetAllLists()
        {
            Logger.LogDebug("Received request to Get all lists");
            var lists = await _handler.GetAllLists();
            return _mapper.Map<List<TransmissionListDTO>>(lists.ToList());
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<TransmissionListDetailDTO>> GetListById(int id)
        {
            try
            {
                Logger.LogDebug("Received request to Get list with id {0}", id);
                var list = await _handler.GetListById(id);
                return _mapper.Map<TransmissionListDetailDTO>(list);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unable to get list with given id {0}", id);
                return NotFound(id);
            }
        }

        [HttpPost("{id}/clear")]
        public async Task<ActionResult> ClearListById(int id)
        {
            try
            {
                Logger.LogDebug("Received request to clear list with id {0}", id);
                await _handler.ClearList(id);
                return NoContent();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unable to clear list with given id {0}", id);
                return NotFound(id);
            }
        }
        
        [HttpPost("{id}/play")]
        public async Task<ActionResult> PlayTransmissionListById(int id)
        {
            try
            {
                Logger.LogDebug("Received request to play list with id {0}", id);
                await _handler.PlayTransmissionList(id);
                return NoContent();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unable to Play list with given id {0}, {1}", id, e.Message);
                return NotFound(id);
            }
        }

        [HttpGet("{id}/events")]
        public async Task<ActionResult<IEnumerable<TransmissionListEventDTO>>> GetEvents(int id)
        {
            try
            {
                Logger.LogDebug("Received request to get events for list with id {0}", id);
                var transmissionEvents = await _handler.GetListEvents(id);
                return _mapper.Map<List<TransmissionListEventDTO>>(transmissionEvents.ToList());
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unable to get events for list with given id {0}", id);
                return NotFound(id);
            }
        }

        [HttpPost("{id}/events")]
        public async Task<ActionResult<TransmissionListEventDTO>> AddEvent(int id, TransmissionListEventUpsertDTO listEvent)
        {
            try
            {
                Logger.LogDebug("Received request to add event to list with id {0} and list event dto {1}", id, JsonSerializer.Serialize(listEvent));
                var createdListEvent = await _handler.AddEvent(id, listEvent);
                return CreatedAtAction(nameof(AddEvent), _mapper.Map<TransmissionListEventDTO>(createdListEvent));
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unable to create event for list with given id {0}, {1}", id, e.Message);
                return NotFound(id);
            }
        }
        
        [HttpPut("{id}/events/{eventId}")]
        public async Task<ActionResult<TransmissionListEventDTO>> UpdateEvent(int id, int eventId, TransmissionListEventUpsertDTO listEvent)
        {
            try
            {
                Logger.LogDebug("Received request to update event to list with id {0} and list event dto {1}", id, JsonSerializer.Serialize(listEvent));
                var updatedListEvent = await _handler.UpdateEventDetails(id, eventId, listEvent);
                return Ok(_mapper.Map<TransmissionListEventDTO>(updatedListEvent));
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unable to update event for list with given id {0}, {1}", id, e.Message);
                return NotFound(id);
            }
        }
        
        [HttpPatch("{id}/events/{eventId}/move")]
        public async Task<ActionResult<TransmissionListEventDTO>> MoveEvent(int id, int eventId, TransmissionListEventMoveDTO listEventMove)
        {
            try
            {
                Logger.LogDebug("Received request to move event {0} on list with id {1} from {2} to {3}", id, eventId, listEventMove.PreviousPosition, listEventMove.TargetPosition);
                var updatedListEvent = await _handler.ChangeEventPosition(id, eventId, listEventMove.PreviousPosition, listEventMove.TargetPosition);
                return _mapper.Map<TransmissionListEventDTO>(updatedListEvent);
            }
            catch(InvalidPositionException e)
            {
                Logger.LogError(e, "Unable to change position of event for list with given id {0}, {1}", id, e.Message);
                return BadRequest(id);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unable to change position of event for list with given id {0}, {1}", id, e.Message);
                return NotFound(id);
            }
        }

        [HttpDelete("{id}/events/{eventId}")]
        public async Task<ActionResult> DeleteEvent(int id, int eventId)
        {
            try
            {
                Logger.LogDebug("Received request to delete event {0} from list with id {1}", id, eventId);
                await _handler.RemoveEvent(id, eventId);
                return NoContent();
            }
            catch(Exception e)
            {
                Logger.LogError(e, "Unable to delete event with given event id {0} and list id {1}", eventId, id);
                return NotFound(id);
            }
        }
    }
}