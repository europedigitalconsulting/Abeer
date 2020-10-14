using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Concurrent;
using Abeer.Shared.Functional;
using System;
using System.Linq;
using Abeer.Data.UnitOfworks;
using System.Collections.Generic;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdController : ControllerBase
    {
        private static readonly ConcurrentBag<AdModel> Ads = new ConcurrentBag<AdModel>();

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdModel>>> List([FromServices] FunctionalUnitOfWork functionalUnitOfWork)
        {
            return Ok(await functionalUnitOfWork.AdRepository.GetAllForAUser(User.NameIdentifier()));
        }

        [AllowAnonymous]
        [HttpGet("Visibled")]
        public async Task<ActionResult<IEnumerable<AdModel>>> GetVisibled([FromServices] FunctionalUnitOfWork functionalUnitOfWork)
        {
            return Ok(await functionalUnitOfWork.AdRepository.GetVisibled());
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<AdModel>> Get(Guid id)
        {
            return await Task.Run(() =>
            {
                var Ad = Ads.FirstOrDefault(o => o.Id == id);
                return Ok(Ad);
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<AdModel>> Create(CreateAdRequestViewModel createAdRequestViewModel)
        {
            return await Task.Run<ActionResult<AdModel>>(() =>
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ad = createAdRequestViewModel.Ad;
                
                if (createAdRequestViewModel.Price.Value == 0)
                {
                    ad.StartDisplayTime = DateTime.Now.AddDays(createAdRequestViewModel.Price.DelayToDisplay);
                    ad.EndDisplayTime = DateTime.Now.AddDays(createAdRequestViewModel.Price.DisplayDuration.GetValueOrDefault(1));
                }

                ad.AdPrice = createAdRequestViewModel.Price;

                Ads.Add(ad);

                return Ok(ad);
            });
        }

        [Authorize]
        [HttpGet("valid/{AdId}")]
        public async Task<ActionResult<AdModel>> Valid(Guid AdId, [FromServices]FunctionalUnitOfWork functionalUnitOfWork)
        {
            var current = Ads.FirstOrDefault(o => o.Id == AdId);
            
            if (current.AdPrice.Value == 0 || !string.IsNullOrEmpty(current.PaymentInformation))
            {
                current.IsValid = true;
                current.ValidateDate = DateTime.UtcNow;
                current.OwnerId = User.NameIdentifier();

                var inserted = await functionalUnitOfWork.AdRepository.AddAsync(current);
                return Ok(inserted);
            }

            return BadRequest();
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update(AdModel ad, [FromServices] FunctionalUnitOfWork functionalUnitOfWork)
        {
            await functionalUnitOfWork.AdRepository.Update(ad);
            return Ok();
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id, [FromServices] FunctionalUnitOfWork functionalUnitOfWork)
        {
            await functionalUnitOfWork.AdRepository.DeleteAsync(id);
            return Ok();
        }


        [Authorize]
        [HttpGet("admin")]
        public async Task<ActionResult<IEnumerable<AdModel>>> GetForAdministration([FromServices] FunctionalUnitOfWork functionalUnitOfWork)
        {
            return Ok(await functionalUnitOfWork.AdRepository.AllAsync());
        }

        [Authorize]
        [HttpPost("admin")]
        public async Task<ActionResult<AdModel>> CreateByAdmin(AdModel adModel, [FromServices] FunctionalUnitOfWork functionalUnitOfWork)
        {
            return Ok(await functionalUnitOfWork.AdRepository.AddAsync(adModel));
        }

        [Authorize]
        [HttpPut("admin")]
        public async Task<IActionResult> UpdateByAdmin(AdModel adModel, [FromServices] FunctionalUnitOfWork functionalUnitOfWork)
        {
            await functionalUnitOfWork.AdRepository.Update(adModel);
            return Ok();
        }
    }
}
