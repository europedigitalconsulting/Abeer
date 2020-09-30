using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cryptocoin.Quorum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EchoController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> Echo()
        {
            return await Task.Run(() => Ok($"OK FROM Server {Environment.MachineName} - {DateTime.UtcNow}"));
        }
    }
}
