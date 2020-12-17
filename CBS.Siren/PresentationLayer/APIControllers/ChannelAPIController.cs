using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using CBS.Siren.PresentationLayer.DTOs;
using System.Collections.Generic;
using CBS.Siren.Application;
using System.Linq;

namespace CBS.Siren.PresentationLayer.APIControllers
{
    [ApiController]
    [Route("api/1/automation/channel")]
    public class ChannelAPIController : ControllerBase
    {
        private ILogger<ChannelAPIController> Logger { get; }
        private IChannelHandler ChannelHandler { get; }

        private readonly IMapper _mapper;

        public ChannelAPIController(ILogger<ChannelAPIController> logger, IMapper mapper, IChannelHandler channelHandler)
        {
            _mapper = mapper;
            ChannelHandler = channelHandler;
            Logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChannelDTO>>> GetChannels()
        {
            Logger.LogDebug("Received request to Get all lists");
            var channels = await ChannelHandler.GetAllChannels();
            return _mapper.Map<List<ChannelDTO>>(channels.ToList());
        }

        [HttpGet("{id}")]
        public Task<ActionResult/*<ChannelDetailsDTO>*/> GetChannelById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public Task<ActionResult/*<ChannelDetailsDTO>*/> CreateChannel(/*ChannelCreationDTO*/)
        {
            throw new NotImplementedException();
        }
        
        [HttpPatch("{id}")]
        public Task<ActionResult/*<ChannelDetailsDTO>*/> UpdateChannel(int id/*, ChannelUpdateDTO*/)
        {
            throw new NotImplementedException();
        }
        
        [HttpDelete("{id}")]
        public Task<ActionResult> DeleteChannel(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPost("{id}")]
        public Task<ActionResult/*<ChannelDetailsDTO>*/> AddListToChannel(int id /*, ListCreationDTO*/)
        {
            throw new NotImplementedException();
        }
        
        [HttpPost("{id}/lists/{listId}")]
        public Task<ActionResult/*<ChannelDetailsDTO>*/> RemoveListFromChannel(int id, int listId /*, ListCreationDTO*/)
        {
            throw new NotImplementedException();
        }
    }
}
