using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abeer.Shared;
using Microsoft.AspNetCore.Authorization;
using Abeer.Data.UnitOfworks;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SocialNetworkController : ControllerBase
    {
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        public SocialNetworkController(FunctionalUnitOfWork functionalUnitOfWork)
        {
            _functionalUnitOfWork = functionalUnitOfWork;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<SocialNetwork>>> GetSocialNetworks()
        {
            return Ok(_functionalUnitOfWork.SocialNetworkRepository.GetNetworks());
        }

        [Authorize]
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PostAsync(SocialNetwork network)
        {
            if (network == null)
                return BadRequest();

            if (!string.IsNullOrEmpty(network.OwnerId) && network.OwnerId != User.NameIdentifier())
                return BadRequest();

            if (string.IsNullOrEmpty(network.OwnerId))
                network.OwnerId = "system";

            await _functionalUnitOfWork.SocialNetworkRepository.AddSocialNetwork(network);
            return Ok();
        }
    }
}
