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
    public class CustomLinkController : ControllerBase
    {
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        public CustomLinkController(FunctionalUnitOfWork functionalUnitOfWork)
        {
            _functionalUnitOfWork = functionalUnitOfWork;
        }

        [HttpDelete("{UserId}/{linkId}")]
        public async Task<IActionResult> Delete(string UserId, string linkId)
        {
            var network = await _functionalUnitOfWork.CustomLinkRepository.FirstOrDefaultAsync(s => s.OwnerId == UserId &&
                s.Id == linkId);

            if (network == null)
                return NotFound();

            await _functionalUnitOfWork.CustomLinkRepository.Remove(network);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(CustomLink network)
        {
            if (network == null)
                return BadRequest();

            if (network.OwnerId != User.NameIdentifier())
                return BadRequest();

            await _functionalUnitOfWork.CustomLinkRepository.AddCustomLink(network);
            return Ok();
        }
    }
}
