using System.Threading.Tasks;
using CBS.Siren.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CBS.Siren.Controllers
{
    [ApiController]
    [Route("api/demo")]
    public class DemoAPIController : ControllerBase
    {
        private readonly ILogger<DemoAPIController> _logger;
        private readonly SirenApplication _application;

        public DemoAPIController(ILogger<DemoAPIController> logger, SirenApplication application)
        {
            _logger = logger;
            _application = application;
        }

        [HttpPost]
        public async Task<IActionResult> Run()
        {
            try
            {
                await _application.RunApplication();    
            }
            catch (System.Exception)
            {
                return BadRequest("Something went wrong running Siren Demo");
            }
            
            return Ok();
        }
    }
}