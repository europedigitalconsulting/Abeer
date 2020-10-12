using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Concurrent;
using Abeer.Shared.Functional;
using System;
using System.Linq;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class OfferController : ControllerBase
    {
        private static readonly ConcurrentBag<OfferModel> Offers = new ConcurrentBag<OfferModel>();

        [HttpGet]
        public async Task<ActionResult<OfferModel>> Get(Guid id)
        {
            return await Task.Run(() =>
            {
                var offer = Offers.FirstOrDefault(o => o.Id == id);
                return Ok(offer);
            });
        }

        [HttpPost]
        public async Task<ActionResult<OfferModel>> Create(CreateOfferRequestViewModel createOfferRequestViewModel)
        {
            return await Task.Run<ActionResult<OfferModel>>(() =>
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var offer = new OfferModel
                {
                    Description = createOfferRequestViewModel.Offer.Description,
                    OwnerId = User.NameIdentifier(),
                    Title = createOfferRequestViewModel.Offer.Title,
                    Url1 = createOfferRequestViewModel.Offer.Url1,
                    Url2 = createOfferRequestViewModel.Offer.Url2,
                    Url3 = createOfferRequestViewModel.Offer.Url3,
                    Url4 = createOfferRequestViewModel.Offer.Url4
                };

                if (createOfferRequestViewModel.Price.Value == 0)
                {
                    offer.StartDisplayTime = DateTime.Now.AddDays(createOfferRequestViewModel.Price.DelayToDisplay);
                    offer.EndDisplayTime = DateTime.Now.AddDays(createOfferRequestViewModel.Price.DisplayDuration.GetValueOrDefault(1));
                }

                offer.OfferPrice = createOfferRequestViewModel.Price;

                Offers.Add(offer);

                return Ok(offer);
            });
        }

        [HttpPut]
        public async Task Valid(OfferModel offerModel)
        {

        }
    }
}
