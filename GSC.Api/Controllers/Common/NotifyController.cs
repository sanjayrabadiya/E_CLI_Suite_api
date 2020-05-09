using GSC.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GSC.Api.Controllers.Common
{
    [Route("api/[controller]")]
    public class NotifyController : ControllerBase
    {
        private readonly IHubContext<Notification> _hub;

        public NotifyController(IHubContext<Notification> hub)
        {
            _hub = hub;
        }

        [HttpGet]
        public IActionResult Get()
        {
            _hub.Clients.All.SendAsync("connected", "connected");
            return Ok(new {Message = "Request Completed"});
        }
    }
}