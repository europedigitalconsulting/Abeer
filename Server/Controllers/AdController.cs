using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Concurrent;
using Abeer.Shared.Functional;
using System;
using System.Linq;
using Abeer.Data.UnitOfworks;
using System.Collections.Generic;
using System.Threading;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdController : ControllerBase
    {
        private readonly FunctionalUnitOfWork functionalUnitOfWork;

        public AdController(FunctionalUnitOfWork functionalUnitOfWork)
        {
            this.functionalUnitOfWork = functionalUnitOfWork;
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdModel>>> List()
        {
            return Ok(await functionalUnitOfWork.AdRepository.GetAllForAUser(User.NameIdentifier()));
        }

        [AllowAnonymous]
        [HttpGet("Visibled")]
        public async Task<ActionResult<IEnumerable<AdModel>>> GetVisibled()
        {
            return Ok(await functionalUnitOfWork.AdRepository.GetVisibled());
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<AdModel>> Get(Guid id)
        {
            var ad = await functionalUnitOfWork.AdRepository.FirstOrDefaultAsync(a => a.Id == id);
            return Ok(ad);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<AdModel>> Create(CreateAdRequestViewModel createAdRequestViewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ad = createAdRequestViewModel.Ad;

            if (createAdRequestViewModel.Price.Value == 0)
            {
                ad.StartDisplayTime = DateTime.Now.AddDays(createAdRequestViewModel.Price.DelayToDisplay);
                ad.EndDisplayTime = DateTime.Now.AddDays(createAdRequestViewModel.Price.DisplayDuration.GetValueOrDefault(1));
            }

            ad.AdPriceId = createAdRequestViewModel.Price.Id;

            await functionalUnitOfWork.AdRepository.AddAsync(ad);
            var entity = await functionalUnitOfWork.AdRepository.FirstOrDefaultAsync(a => a.Id == ad.Id);
            return Ok(entity);
        }

        [Authorize]
        [HttpGet("valid/{AdId}")]
        public async Task<ActionResult<AdModel>> Valid(Guid AdId, [FromServices]FunctionalUnitOfWork functionalUnitOfWork)
        {
            var current = await functionalUnitOfWork.AdRepository.FirstOrDefaultAsync(o => o.Id == AdId);
            
            if (current.AdPrice.Value == 0 || !string.IsNullOrEmpty(current.PaymentInformation))
            {
                current.IsValid = true;
                current.ValidateDate = DateTime.UtcNow;
                current.OwnerId = User.NameIdentifier();

                await functionalUnitOfWork.AdRepository.Update(current);
                return Ok(current);
            }

            return BadRequest();
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update(AdModel ad)
        {
            await functionalUnitOfWork.AdRepository.Update(ad);
            return Ok();
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            await functionalUnitOfWork.AdRepository.DeleteAsync(id);
            return Ok();
        }


        [Authorize]
        [HttpGet("admin")]
        public async Task<ActionResult<IEnumerable<AdModel>>> GetForAdministration()
        {
            return Ok(await functionalUnitOfWork.AdRepository.AllAsync());
        }

        [Authorize]
        [HttpPost("admin")]
        public async Task<ActionResult<AdModel>> CreateByAdmin(AdModel adModel)
        {
            return Ok(await functionalUnitOfWork.AdRepository.AddAsync(adModel));
        }

        [Authorize]
        [HttpPut("admin")]
        public async Task<IActionResult> UpdateByAdmin(AdModel adModel)
        {
            await functionalUnitOfWork.AdRepository.Update(adModel);
            return Ok();
        }
    }
}
