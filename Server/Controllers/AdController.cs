using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Abeer.Shared.Functional;
using System;
using System.Collections.Concurrent;
using Abeer.Data.UnitOfworks;
using System.Collections.Generic;
using System.Linq;
using Abeer.Shared;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = "OnlySubscribers")]
    [ApiController]
    public class AdsController : ControllerBase
    {
        private readonly FunctionalUnitOfWork functionalUnitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly Random rdm = new Random();

        public AdsController(FunctionalUnitOfWork functionalUnitOfWork, UserManager<ApplicationUser> userManager)
        {
            this.functionalUnitOfWork = functionalUnitOfWork;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdModel>>> List()
        {
            return Ok(await functionalUnitOfWork.AdRepository.GetAllForAUser(User.NameIdentifier()));
        }

        [HttpGet("notvalid")]
        public async Task<ActionResult<IEnumerable<AdModel>>> NotValid()
        {
            return Ok(await functionalUnitOfWork.AdRepository.GetAllForAUser(User.NameIdentifier(), false));
        }


        [HttpGet("Visibled")]
        public async Task<ActionResult<IEnumerable<AdModel>>> GetVisibled()
        {
            return Ok(await functionalUnitOfWork.AdRepository.GetVisibled());
        }

        [HttpGet("country")]
        public async Task<ActionResult<IEnumerable<AdModel>>> GetVisibledCountry()
        {
            return Ok(await functionalUnitOfWork.AdRepository.GetVisibledCountry(User.Country()));
        }

        [HttpGet("freinds")]
        public async Task<ActionResult<IEnumerable<AdModel>>> GetVisibledFreinds()
        {
            var contacts = await functionalUnitOfWork.ContactRepository.Where(c => c.OwnerId == User.NameIdentifier());
            
            var ads = new ConcurrentDictionary<string, IList<AdModel>>();
            
            Parallel.ForEach(contacts, async contact =>
            {
                ads.TryAdd(contact.UserId, await functionalUnitOfWork.AdRepository.GetVisibledUser(contact.UserId));
            });

            var result = ads.Values.ToList().SelectMany(a => a);

            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<AdViewModel>> Get(Guid id)
        {
            var ad = (AdViewModel)(await functionalUnitOfWork.AdRepository.FirstOrDefault(a => a.Id == id));
           
            ad.Owner = await _userManager.FindByIdAsync(ad.OwnerId);
            ad.Owner.PhotoUrl ??= new GravatarUrlExtension.Gravatar().GetImageSource(ad.Owner.Email);

            var authorAds = await functionalUnitOfWork.AdRepository.GetVisibledUser(ad.OwnerId);

            ad.Owner.AdsCount = authorAds.Count;

            ad.OtherAds = authorAds.Select(a=>new ListAdViewModel{Id = a.Id, ImageUrl1 =  a.ImageUrl1, Title = a.Title, OrderNumber = a.OrderNumber})
                .OrderBy(a=>a.OrderNumber).ToList();

            ad.Owner.SocialNetworkConnected =
                await functionalUnitOfWork.SocialNetworkRepository.GetSocialNetworkLinks(ad.OwnerId);
            
            ad.Owner.CustomLinks = await functionalUnitOfWork.CustomLinkRepository.GetCustomLinkLinks(ad.OwnerId);

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

                ad.OwnerId = User.NameIdentifier();
                ad.Country = User.Country();

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

                functionalUnitOfWork.SaveChanges();
                return Ok(current);
            }

            return BadRequest();
        }
         
        [HttpPut]
        public async Task<IActionResult> Update(AdModel ad)
        {
            var model = await functionalUnitOfWork.AdRepository.FirstOrDefault(m => m.Id == ad.Id);
            
            model.Description = ad.Description;
            model.ImageUrl1 = ad.ImageUrl1;
            model.ImageUrl2 = ad.ImageUrl2;
            model.ImageUrl3 = ad.ImageUrl3;
            model.ImageUrl4 = ad.ImageUrl4;
            model.Title = ad.Title;
            model.Price = ad.Price;
            model.Currency = ad.Currency ?? "USD";
            model.Url1 = ad.Url1;
            model.Url2 = ad.Url2;
            model.Url3 = ad.Url3;
            model.Url4 = ad.Url4;

            functionalUnitOfWork.SaveChanges();
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
