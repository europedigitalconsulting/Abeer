using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abeer.Shared;
using Microsoft.AspNetCore.Authorization;

namespace Abeer.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SocialNetworkController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<SocialNetwork>>> GetSocialNetworks()
        {
            var socialNetworks = new List<SocialNetwork>
            {
                new SocialNetwork {Name = "Facebook", Logo = "facebook"},
                new SocialNetwork {Name = "Twitter", Logo = "twitter"},
                new SocialNetwork {Name = "Instagram", Logo = "instagram"},
                new SocialNetwork {Name = "Whatsapp", Logo = "success"},
                new SocialNetwork {Name = "Linkedin", Logo = "linkedin"},
                new SocialNetwork {Name = "Microsoft", Logo = "microsoft"},
                new SocialNetwork {Name = "Apple", Logo = "twitter"}

            };
            return Ok(socialNetworks);
        }
    }
}
