using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Abeer.Shared.Functional;
using System;
using System.Collections.Concurrent;
using Abeer.Data.UnitOfworks;
using System.Collections.Generic;
using System.Linq;
using Abeer.Services;
using Abeer.Shared;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Abeer.Shared.Technical;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Extensions;
using static Abeer.Services.TemplateRenderManager;
using Microsoft.AspNetCore.Hosting;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = "OnlySubscribers")]
    [ApiController]
    public class AdsController : ControllerBase
    {
        private readonly FunctionalUnitOfWork functionalUnitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EventTrackingService _eventTrackingService;
        private readonly NotificationService _notificationService;
        private readonly IConfiguration _configuration;
        private readonly UrlShortner _urlShortner;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailSenderService _emailSender;

        private readonly Random rdm = new Random();
        public AdsController(FunctionalUnitOfWork functionalUnitOfWork, UserManager<ApplicationUser> userManager,
            EventTrackingService eventTrackingService, NotificationService notificationService, 
            IConfiguration configuration, UrlShortner urlShortner, IServiceProvider serviceProvider, IWebHostEnvironment env, IEmailSenderService emailSender)
        {
            this.functionalUnitOfWork = functionalUnitOfWork;
            _userManager = userManager;
            _eventTrackingService = eventTrackingService;
            _notificationService = notificationService;
            _configuration = configuration;
            _urlShortner = urlShortner;
            _serviceProvider = serviceProvider;
            _env = env;
            _emailSender = emailSender;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdModel>>> List()
        {
            return Ok(await functionalUnitOfWork.AdRepository.GetAllForAUser(User.NameIdentifier()));
        }

        [HttpGet("notvalid")]
        public async Task<ActionResult<IEnumerable<AdModel>>> NotValid()
        {
            var viewApplicationUser = (ViewApplicationUser)User;
    
            if (viewApplicationUser.IsAdmin)
                return Ok(await functionalUnitOfWork.AdRepository.GetAll(false));
            else
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

            ad.OtherAds = authorAds.Select(a => new ListAdViewModel { Id = a.Id, ImageUrl1 = a.ImageUrl1, Title = a.Title, OrderNumber = a.OrderNumber })
                .OrderBy(a => a.OrderNumber).ToList();

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
            var applicationUser = (ViewApplicationUser)User;

            if (createAdRequestViewModel.Price != null)
            {
                ad.OrderNumber = string.Concat(DateTime.UtcNow.ToString("yyyMMddHHmmss"), rdm.Next(100000, 999999));

                Subscription subscription = null;
                SubscriptionPack pack = null;
                
                int delayToDisplay = 0;
                int displayDuration = 0;

                if (applicationUser.IsAdmin || applicationUser.IsManager)
                {
                    delayToDisplay = 0;
                    ad.IsValid = true;
                    ad.ValidateDate = DateTime.UtcNow;
                    displayDuration = 7;
                }

                else if (!applicationUser.IsUnlimited)
                {
                    subscription = await functionalUnitOfWork.SubscriptionRepository.GetLatestSubscriptionForUser(User.NameIdentifier());
                    
                    if (subscription != null)
                        pack = await functionalUnitOfWork.SubscriptionPackRepository.FirstOrDefault(p => p.Id == subscription.SubscriptionPackId);

                    if (pack != null)
                    {
                        if (pack.Label == "Standard")
                        {
                            delayToDisplay = 2;
                            displayDuration = 2;
                        }
                        else
                        {
                            delayToDisplay = 5;
                            displayDuration = 5;
                        }
                    }
                    else
                    {
                        delayToDisplay = 2;
                        displayDuration = 2;
                    }
                }

                var priceDisplayDuration = createAdRequestViewModel.Price.DisplayDuration.GetValueOrDefault(0);
                var priceDelayToDisplay = createAdRequestViewModel.Price.DelayToDisplay;

                if (priceDelayToDisplay < delayToDisplay)
                    delayToDisplay = priceDelayToDisplay;

                if (priceDisplayDuration > displayDuration)
                    displayDuration = priceDisplayDuration;

                ad.StartDisplayTime = DateTime.UtcNow.AddDays(delayToDisplay);
                ad.EndDisplayTime = ad.StartDisplayTime.AddDays(displayDuration);

                ad.AdPriceId = createAdRequestViewModel.Price.Id;
            }

            ad.OwnerId = applicationUser.Id;
            ad.Country = applicationUser.Country;

            await functionalUnitOfWork.AdRepository.Add(ad);
            var entity = await functionalUnitOfWork.AdRepository.FirstOrDefault(a => a.Id == ad.Id);
            
            await _eventTrackingService.Create(applicationUser.Id, "Ad", "create");

            return Ok(entity);
        }

        [HttpGet("valid/{AdId}")]
        public async Task<ActionResult<AdModel>> Valid(Guid AdId, [FromServices] FunctionalUnitOfWork functionalUnitOfWork)
        {
            var current = await functionalUnitOfWork.AdRepository.FirstOrDefault(o => o.Id == AdId);

            var price = (await functionalUnitOfWork.AdPriceRepository.All()).FirstOrDefault(p => p.Id == current.AdPriceId);

            if (price?.Value == 0 || !string.IsNullOrEmpty(current.PaymentInformation))
            {
                current.IsValid = true;
                current.ValidateDate = DateTime.UtcNow;
                current.OwnerId = User.NameIdentifier();
                await functionalUnitOfWork.AdRepository.Update(current);
                functionalUnitOfWork.SaveChanges();
                await _eventTrackingService.Create(current.OwnerId, "Ad", "validate");
                await _notificationService.Create(current.OwnerId, "validateAd", "/ad/myads");

                await SendEmailTemplate(current);

                return Ok(current);
            }

            return BadRequest();
        }

        private async Task SendEmailTemplate(AdModel adModel)
        {
            var callbackUrl = $"{Request.Scheme}://{Request.Host}/ads/details/{adModel.Id}";

            var frontWebSite = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var logoUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/assets/img/logo_full.png");
            var unSubscribeUrl = await _urlShortner.CreateUrl(Request.Scheme, Request.Host, UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/Account/UnSubscribe"));

            callbackUrl = await _urlShortner.CreateUrl(Request.Scheme, Request.Host, callbackUrl);

            var parameters = new Dictionary<string, string>()
                        {
                            {"title", adModel.Title },
                            {"frontWebSite", frontWebSite },
                            {"logoUrl", logoUrl },
                            {"unSubscribeUrl", unSubscribeUrl },
                            {"callbackUrl", callbackUrl }
                        };

            var message = GenerateHtmlTemplate(_serviceProvider, _env.WebRootPath, "valid-ad", parameters);
            var user = await _userManager.FindByIdAsync(adModel.OwnerId);
            
            await _emailSender.SendEmailAsync(user.Email, adModel.Title, message);
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
            if (adModel.AdPrice == null)
            {
                var price = (await functionalUnitOfWork.AdPriceRepository.All()).FirstOrDefault(p => p.PriceName == "free");

                if (price == null)
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
