using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abeer.Shared;
using Microsoft.AspNetCore.Authorization;
using Abeer.Data.UnitOfworks;
using System;
using Abeer.Services;

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
        public async Task<IActionResult> Delete(string UserId, Guid linkId)
        {
            var network = await _functionalUnitOfWork.CustomLinkRepository.FirstOrDefault(s => s.OwnerId == UserId &&
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

        [HttpPut]
        public async Task<IActionResult> PutAsync(CustomLink network)
        {
            if (network == null)
                return BadRequest();

            if (network.OwnerId != User.NameIdentifier())
                return BadRequest();

            await _functionalUnitOfWork.CustomLinkRepository.Update(network);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            var network = await _functionalUnitOfWork.CustomLinkRepository.FirstOrDefault(c => c.Id == id);

            if (network == null)
                return NotFound();

            if (network.OwnerId != User.NameIdentifier())
                return BadRequest();

            await _functionalUnitOfWork.CustomLinkRepository.Remove(network);
            return Ok();
        }
    }
}
