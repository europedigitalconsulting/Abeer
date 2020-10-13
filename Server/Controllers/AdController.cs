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

                var Ad = new AdModel
                {
                    Description = createAdRequestViewModel.Ad.Description,
                    OwnerId = User.NameIdentifier(),
                    Title = createAdRequestViewModel.Ad.Title,
                    Url1 = createAdRequestViewModel.Ad.Url1,
                    Url2 = createAdRequestViewModel.Ad.Url2,
                    Url3 = createAdRequestViewModel.Ad.Url3,
                    Url4 = createAdRequestViewModel.Ad.Url4
                };

                if (createAdRequestViewModel.Price.Value == 0)
                {
                    Ad.StartDisplayTime = DateTime.Now.AddDays(createAdRequestViewModel.Price.DelayToDisplay);
                    Ad.EndDisplayTime = DateTime.Now.AddDays(createAdRequestViewModel.Price.DisplayDuration.GetValueOrDefault(1));
                }

                Ad.AdPrice = createAdRequestViewModel.Price;

                Ads.Add(Ad);

                return Ok(Ad);
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
    }
}
