using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Orleans;
using SpawnCloud.ChatService.Contracts.Exceptions;
using SpawnCloud.ChatService.Contracts.Models;
using SpawnCloud.ChatService.Grains;

namespace SpawnCloud.ChatService.API.Controllers
{
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class ChannelController : ControllerBase
    {
        private readonly ILogger<ChannelController> _logger;
        private readonly IClusterClient _orleansClient;

        public ChannelController(ILogger<ChannelController> logger, IClusterClient orleansClient)
        {
            _logger = logger;
            _orleansClient = orleansClient;
        }

        [HttpGet("{channelId}")]
        public async Task<IActionResult> GetChannel(Guid channelId)
        {
            var chatChannelGrain = _orleansClient.GetGrain<IChatChannelGrain>(channelId);
            try
            {
                var channelDescription = await chatChannelGrain.GetDescription();
                return Ok(channelDescription);
            }
            catch (ChannelDoesNotExistException)
            {
                return NotFound();
            }
        }
        
        [HttpPut]
        public async Task<IActionResult> CreateChannel([FromBody] ChannelSettings channelSettings)
        {
            var channelId = Guid.NewGuid();
            var chatChannelGrain = _orleansClient.GetGrain<IChatChannelGrain>(channelId);
            await chatChannelGrain.InitializeChannel(channelSettings);
            var newChannelDescription = await chatChannelGrain.GetDescription();

            return CreatedAtAction("CreateChannel", new { channelId = channelId }, newChannelDescription);
        }
    }
}
