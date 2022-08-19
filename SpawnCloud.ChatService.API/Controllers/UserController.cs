using Microsoft.AspNetCore.Mvc;
using Orleans;
using SpawnCloud.ChatService.Grains;

namespace SpawnCloud.ChatService.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IClusterClient _orleansClient;

        public UserController(ILogger<UserController> logger, IClusterClient orleansClient)
        {
            _logger = logger;
            _orleansClient = orleansClient;
        }

        /// <summary>
        /// List all channels that a user belongs to.
        /// </summary>
        [HttpGet("{userId}/channels")]
        public async Task<IActionResult> ListChannels(Guid userId)
        {
            var chatUserGrain = _orleansClient.GetGrain<IChatUserGrain>(userId);
            var channels = await chatUserGrain.ListChannels();
            return Ok(channels);
        }
    }
}