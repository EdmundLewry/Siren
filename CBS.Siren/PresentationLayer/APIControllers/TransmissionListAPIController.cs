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
        public IMapper _mapper;

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

        [HttpGet("{id}/events")]
        public async Task<ActionResult<IEnumerable<TransmissionListEventDTO>>> GetEvents(string id)
        {
            try
            {
                var transmissionEvents = await _handler.GetListEvents(id);
                return _mapper.Map<List<TransmissionListEventDTO>>(transmissionEvents.ToList());
            }
            catch(Exception)
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
                return CreatedAtAction(nameof(AddEvent), createdListEvent);
            }
            catch(Exception)
            {
                return NotFound(id);
            }
        }
    }
}