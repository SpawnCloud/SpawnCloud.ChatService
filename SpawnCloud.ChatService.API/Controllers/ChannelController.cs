using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;

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
    }
}
