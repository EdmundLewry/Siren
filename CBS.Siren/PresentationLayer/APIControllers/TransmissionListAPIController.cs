using System.Collections.Generic;
using System.Threading.Tasks;
using CBS.Siren.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using AutoMapper;
using CBS.Siren.DTO;

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
    }
}