using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Abeer.Shared.Functional;
using System;
using Abeer.Data.UnitOfworks;
using System.Collections.Generic;
using System.Linq;

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
            var ad = await functionalUnitOfWork.AdRepository.FirstOrDefault(a => a.Id == id);
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

            await functionalUnitOfWork.AdRepository.Add(ad);
            var entity = await functionalUnitOfWork.AdRepository.FirstOrDefault(a => a.Id == ad.Id);
            return Ok(entity);
        }

        [Authorize]
        [HttpGet("valid/{AdId}")]
        public async Task<ActionResult<AdModel>> Valid(Guid AdId, [FromServices]FunctionalUnitOfWork functionalUnitOfWork)
        {
            var current = await functionalUnitOfWork.AdRepository.FirstOrDefault(o => o.Id == AdId);
            
            var price = (await functionalUnitOfWork.AdPriceRepository.All()).FirstOrDefault(p => p.Id == current.AdPriceId);

            if (price?.Value == 0 || !string.IsNullOrEmpty(current.PaymentInformation))
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
            await functionalUnitOfWork.AdRepository.Delete(id);
            return Ok();
        }


        [Authorize]
        [HttpGet("admin")]
        public async Task<ActionResult<IEnumerable<AdModel>>> GetForAdministration()
        {
            return Ok(await functionalUnitOfWork.AdRepository.All());
        }

        [Authorize]
        [HttpPost("admin")]
        public async Task<ActionResult<AdModel>> CreateByAdmin(AdModel adModel)
        {
            return Ok(await functionalUnitOfWork.AdRepository.Add(adModel));
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
