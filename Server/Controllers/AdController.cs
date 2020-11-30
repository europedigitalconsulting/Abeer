using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Abeer.Shared.Functional;
using System;
using Abeer.Data.UnitOfworks;
using System.Collections.Generic;
using System.Linq;
using Abeer.Shared.ViewModels;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = "OnlySubscribers")]
    [ApiController]
    public class AdssController : ControllerBase
    {
        private readonly FunctionalUnitOfWork functionalUnitOfWork;

        private readonly Random rdm = new Random();
        public AdssController(FunctionalUnitOfWork functionalUnitOfWork)
        {
            this.functionalUnitOfWork = functionalUnitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdModel>>> List()
        {
            return Ok(await functionalUnitOfWork.AdRepository.GetAllForAUser(User.NameIdentifier()));
        }
         
        [HttpGet("Visibled")]
        public async Task<ActionResult<IEnumerable<AdModel>>> GetVisibled()
        {
            return Ok(await functionalUnitOfWork.AdRepository.GetVisibled());
        }
         
        [HttpGet("{id}")]
        public async Task<ActionResult<AdModel>> Get(Guid id)
        {
            var ad = await functionalUnitOfWork.AdRepository.FirstOrDefault(a => a.Id == id);
            return Ok(ad);
        }

        [HttpPost] 
        public async Task<ActionResult<AdModel>> Create(CreateAdRequestViewModel createAdRequestViewModel)
        { 
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ad = createAdRequestViewModel.Ad;

                if (createAdRequestViewModel.Price != null)
                {
                    ad.OrderNumber = string.Concat(DateTime.UtcNow.ToString("yyyMMddHHmmss"), rdm.Next(100000, 999999));
                    if (createAdRequestViewModel.Price.Value == 0)
                    {
                        ad.StartDisplayTime = DateTime.Now.AddDays(createAdRequestViewModel.Price.DelayToDisplay);
                        ad.EndDisplayTime = DateTime.Now.AddDays(createAdRequestViewModel.Price.DisplayDuration.GetValueOrDefault(1));
                    }

                    ad.AdPriceId = createAdRequestViewModel.Price.Id;
                }

                await functionalUnitOfWork.AdRepository.Add(ad);
                var entity = await functionalUnitOfWork.AdRepository.FirstOrDefault(a => a.Id == ad.Id);
                return Ok(entity); 
        }
         
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
         
        [HttpPut]
        public async Task<IActionResult> Update(AdModel ad)
        {
            await functionalUnitOfWork.AdRepository.Update(ad);
            return Ok();
        }
         
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await functionalUnitOfWork.AdRepository.Delete(id);
            return Ok();
        }
         
        [HttpGet("admin")]
        public async Task<ActionResult<IEnumerable<AdModel>>> GetForAdministration()
        {
            return Ok(await functionalUnitOfWork.AdRepository.All());
        }
         
        [HttpPost("admin")]
        public async Task<ActionResult<AdModel>> CreateByAdmin(AdModel adModel)
        {
            if(adModel.AdPrice == null)
            {
                var price = (await functionalUnitOfWork.AdPriceRepository.All()).FirstOrDefault(p=>p.PriceName == "free");

                if(price == null)
                {
                    price = new AdPrice { Value = 0, DelayToDisplay = 2, DisplayDuration = 2, PriceName = "free" };
                    await functionalUnitOfWork.AdPriceRepository.Add(price);
                }

                adModel.AdPrice = price;
                adModel.AdPriceId = price.Id;
            }

            return Ok(await functionalUnitOfWork.AdRepository.Add(adModel));
        }
         
        [HttpPut("admin")]
        public async Task<IActionResult> UpdateByAdmin(AdModel adModel)
        {
            await functionalUnitOfWork.AdRepository.Update(adModel);
            return Ok();
        }
    }
}
