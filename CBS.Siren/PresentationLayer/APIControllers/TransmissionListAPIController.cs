using System.Collections.Generic;
using System.Threading.Tasks;
using CBS.Siren.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using AutoMapper;
using CBS.Siren.DTO;
using System;

namespace CBS.Siren.Controllers
{
    [ApiController]
    [Route("api/1/transmissionlist")]
    public class TransmissionListAPIController : ControllerBase
    {
        private readonly ILogger<TransmissionListAPIController> _logger;
        private readonly ITransmissionListHandler _handler;
        private readonly IMapper _mapper;

        public TransmissionListAPIController(ILogger<TransmissionListAPIController> logger, ITransmissionListHandler handler, IMapper mapper)
        {
            _logger = logger;
            _handler = handler;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransmissionListDTO>>> GetAllLists()
        {
            var lists = await _handler.GetAllLists();
            return _mapper.Map<List<TransmissionListDTO>>(lists.ToList());
        }

        [HttpPost("{id}/clear")]
        public async Task<ActionResult> ClearList(string id)
        {
            try
            {
                await _handler.ClearList(id);
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound(id);
            }
        }

        [HttpGet("{id}/events")]
        public async Task<ActionResult<IEnumerable<TransmissionListEventDTO>>> GetEvents(string id)
        {
            try
            {
                var transmissionEvents = await _handler.GetListEvents(id);
                return _mapper.Map<List<TransmissionListEventDTO>>(transmissionEvents.ToList());
            }
            catch (Exception)
            {
                return NotFound(id);
            }
        }

        [HttpPost("{id}/events")]
        public async Task<ActionResult<TransmissionListEventDTO>> AddEvent(string id, TransmissionListEventCreationDTO listEvent)
        {
            try
            {
                var createdListEvent = await _handler.AddEvent(id, listEvent);
                return CreatedAtAction(nameof(AddEvent), _mapper.Map<TransmissionListEventDTO>(createdListEvent));
            }
            catch (Exception)
            {
                return NotFound(id);
            }
        }

        [HttpDelete("{id}/events/{eventId}")]
        public async Task<ActionResult> DeleteEvent(string id, string eventId)
        {
            try
            {
                await _handler.RemoveEvent(id, eventId);
                return NoContent();
            }
            catch(Exception)
            {
                return NotFound(id);
            }
        }
    }
}