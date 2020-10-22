using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abeer.Shared;
using Microsoft.AspNetCore.Authorization;
using Abeer.Data.UnitOfworks;

namespace Abeer.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SocialNetworkController : ControllerBase
    {
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        public SocialNetworkController(FunctionalUnitOfWork functionalUnitOfWork)
        {
            _functionalUnitOfWork = functionalUnitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<List<SocialNetwork>>> GetSocialNetworks()
        {
            var socialNetworks = await Task.Run(() =>
            {
                return new List<SocialNetwork>
                {
                    new SocialNetwork{BackgroundColor = "bg-primary", Logo = "fab fa-facebook-square", Name="Facebook"},
                    new SocialNetwork{BackgroundColor = "bg-danger", Logo = "fab fa-youtube-square", Name="Youtube"},
                    new SocialNetwork{BackgroundColor = "bg-danger", Logo = "fab fa-instagram-square", Name="Instagram"},
                    new SocialNetwork{BackgroundColor = "bg-primary", Logo = "fab fa-twitter-square", Name="Twitter"},
                    new SocialNetwork{BackgroundColor = "bg-secondary", Logo = "fab fa-pinterest-square", Name="Pinterest"},
                    new SocialNetwork{BackgroundColor = "bg-primary", Logo = "fab fa-linkedin", Name="Linkedin"}
                };
            });

            return Ok(socialNetworks);
        }

        [HttpDelete("{UserId}/{networkName}")]
        public async Task<IActionResult> Delete(string UserId, string networkName)
        {
            var network = await _functionalUnitOfWork.SocialNetworkRepository.FirstOrDefault(s => s.OwnerId == UserId &&
                s.Name == networkName);

            if (network == null)
                return NotFound();

            await _functionalUnitOfWork.SocialNetworkRepository.Remove(network);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(SocialNetwork network)
        {
            if (network == null)
                return BadRequest();

            if (network.OwnerId != User.NameIdentifier())
                return BadRequest();

            await _functionalUnitOfWork.SocialNetworkRepository.AddSocialNetwork(network);
            return Ok();
        }
    }
}
