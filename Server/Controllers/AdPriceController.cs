using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Abeer.Shared.Functional;
using Abeer.Data.UnitOfworks;
using System.Collections.Generic;
using System;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdPriceController : ControllerBase
    {
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;

        public AdPriceController(FunctionalUnitOfWork functionalUnitOfWork)
        {
            _functionalUnitOfWork = functionalUnitOfWork;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdPrice>>> List()
        {
            var data = await _functionalUnitOfWork.AdPriceRepository.AllAsync();
            return Ok(data);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(AdPrice adPrice)
        {
            var entity = await _functionalUnitOfWork.AdPriceRepository.AddAsync(adPrice);
            return Ok(entity);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update(AdPrice adPrice)
        {
            await _functionalUnitOfWork.AdPriceRepository.Update(adPrice);
            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _functionalUnitOfWork.AdPriceRepository.DeleteAsync(id);
            return Ok();
        }
    }
}
