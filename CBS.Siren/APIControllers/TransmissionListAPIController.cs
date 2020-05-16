using System.Collections.Generic;
using System.Threading.Tasks;
using CBS.Siren.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace CBS.Siren.Controllers
{
    [ApiController]
    [Route("api/1/transmissionlist")]
    public class TransmissionListAPIController : ControllerBase
    {
        private readonly ILogger<TransmissionListAPIController> _logger;
        private readonly ITransmissionListHandler _handler;

        public TransmissionListAPIController(ILogger<TransmissionListAPIController> logger, ITransmissionListHandler handler)
        {
            _logger = logger;
            _handler = handler;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransmissionList>>> GetAllLists()
        {
            var lists = await _handler.GetAllLists();
            return lists.ToList();
        }
        
        // [HttpGet("{id}")]
        // public async Task<ActionResult<TransmissionList>> GetListById()
        // {
            
        // }
    }
}