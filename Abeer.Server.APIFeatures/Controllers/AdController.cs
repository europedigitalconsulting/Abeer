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
using Abeer.Ads.Shared;
using Abeer.Ads.Data;
using Abeer.Shared.Technical;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Extensions;
using static Abeer.Services.TemplateRenderManager;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using AutoMapper;
using Abeer.Ads.Models;
using Microsoft.EntityFrameworkCore;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdsController : ControllerBase
    {
        private readonly AdsUnitOfWork _adsUnitOfWork;
        private readonly FunctionalUnitOfWork functionalUnitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EventTrackingService _eventTrackingService;
        private readonly NotificationService _notificationService;
        private readonly IConfiguration _configuration;
        private readonly UrlShortner _urlShortner;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailSenderService _emailSender;
        private readonly IMapper _mapper;

        private readonly Random rdm = new Random();
        public AdsController(AdsUnitOfWork adsUnitOfWork, FunctionalUnitOfWork functionalUnitOfWork, UserManager<ApplicationUser> userManager,
            EventTrackingService eventTrackingService, NotificationService notificationService, IMapper mapper,
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
            _adsUnitOfWork = adsUnitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<MyAdsViewModel>> List()
        {
            MyAdsViewModel vm = new MyAdsViewModel();
            vm.Families = (await _adsUnitOfWork.FamiliesRepository.GetAll()).ToList();
            var countryTmp = (await functionalUnitOfWork.CountriesRepository.GetCountries("fr")).ToList();
            vm.Countries = _mapper.Map<List<Country>, List<CountryViewModel>>(countryTmp);
            var adtmp = (await functionalUnitOfWork.AdRepository.GetAll(true)).ToList();
            vm.Ads = _mapper.Map<List<AdModel>, List<AdViewModel>>(adtmp);

            return Ok(vm);
        }

        [HttpPost("search")]
        public async Task<ActionResult> search(MyAdsViewModel model)
        {
            try
            {
                var listIdsUsers = _userManager.Users.Where(x => (model.ListCodeCountrySelected.Contains(x.Country) || model.ListCodeCountrySelected.Count == 0) && x.EmailConfirmed).Select(c => c.Id).ToList();
                var listAd = await functionalUnitOfWork.AdRepository.Where(x => listIdsUsers.Contains(x.OwnerId) && x.Title.Contains(model.searchTxt));
                var listFiltered = await _adsUnitOfWork.CategoryAdRepository.Where(x => listAd.Select(c => c.Id).Contains(x.AdId) 
                                                                                        && (model.ListIdCategorySelected.Contains(x.CategoryId) || model.ListIdCategorySelected.Count == 0));
                listAd = listAd.Where(x => listFiltered.Select(c => c.AdId).Contains(x.Id)).ToList();

                return Ok(listAd);
            }
            catch (Exception ex )
            {

                throw;
            }
            
            return Ok();
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

        [HttpGet("family/{familyCode}")]
        public async Task<ActionResult<IEnumerable<AdModel>>> GetVisibledFamily(string familyCode)
        {
            var visibled = await functionalUnitOfWork.AdRepository.GetVisibled();

            var family = await _adsUnitOfWork.FamiliesRepository.GetByCode(familyCode);
            var categories = await _adsUnitOfWork.CategoriesRepository.FilterByFamilies(new List<Guid> { family.FamilyId });
            var adCategories = await _adsUnitOfWork.CategoryAdRepository.GetAllByCategoriesId(categories.Select(c => c.CategoryId));

            var result = visibled.Where(a => adCategories.Any(ac => ac.AdId == a.Id)).ToList();

            return Ok(result);
        }

        [HttpGet("category/{categoryCode}")]
        public async Task<ActionResult<IEnumerable<AdModel>>> GetVisibledCategoryCode(string categoryCode)
        {
            return Ok(await functionalUnitOfWork.AdRepository.GetVisibledCountry(User.Country()));
        }


        [HttpGet("author/{authorId}")]
        public async Task<ActionResult<IEnumerable<AdModel>>> GetVisibledAuthor(string authorId)
        {
            return Ok(await functionalUnitOfWork.AdRepository.GetVisibledUser(authorId));
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
            if (id == Guid.Empty)
                return BadRequest();

            if (Request.Query.Any())
            {
                if (QueryHelpers.ParseQuery(Request.QueryString.Value).TryGetValue("social", out var _social))
                {
                    await _eventTrackingService.Create(User.NameIdentifier(), $"ViewAdFromSocial#{_social.ToString().ToLower()}", id.ToString());
                }
            }

            var model = await functionalUnitOfWork.AdRepository.FirstOrDefault(a => a.Id == id);

            if (model == null)
                return NotFound();

            var ad = (AdViewModel)(model);

            ad.Owner = await _userManager.FindByIdAsync(ad.OwnerId);
            ad.Owner.PhotoUrl ??= new GravatarUrlExtension.Gravatar().GetImageSource(ad.Owner.Email);

            await _eventTrackingService.Create(User.NameIdentifier(), "ViewAd", id.ToString());

            model.ViewCount += 1;
            await functionalUnitOfWork.AdRepository.Update(model);
            ad.ViewCount = model.ViewCount;

            var authorAds = await functionalUnitOfWork.AdRepository.GetVisibledUser(ad.OwnerId);

            ad.Owner.AdsCount = authorAds.Count;

            ad.OtherAds = authorAds.Select(a => new ListAdViewModel { Id = a.Id, ImageUrl1 = a.ImageUrl1, Title = a.Title, OrderNumber = a.OrderNumber })
                .OrderBy(a => a.OrderNumber).ToList();

            ad.Owner.SocialNetworkConnected =
                await functionalUnitOfWork.SocialNetworkRepository.GetSocialNetworkLinks(ad.OwnerId);

            ad.Owner.CustomLinks = await functionalUnitOfWork.CustomLinkRepository.GetCustomLinkLinks(ad.OwnerId);

            var categorieIds = await _adsUnitOfWork.CategoryAdRepository.GetAllIdCatByAdId(ad.Id);

            List<string> categories = new();
            string family = string.Empty;

            foreach (Guid categoryId in categorieIds)
            {
                var category = await _adsUnitOfWork.CategoriesRepository.Get(categoryId);

                if (string.IsNullOrEmpty(family))
                {
                    var familyId = category.FamilyId;
                    var f = await _adsUnitOfWork.FamiliesRepository.Get(familyId);
                    family = f.Code;
                }

                categories.Add(category.Code);
            }

            ad.Family = family;
            ad.Categories = categories;
            ad.ListIdCategory = categorieIds;

            var events = await _eventTrackingService.Where(c => c.Category == "ViewAd" && c.Key == id.ToString());

            if (events.Any())
            {
                var contactIds = events.OrderByDescending(e => e.CreatedDate).Select(e => e.UserId).Distinct().Take(10).ToArray();
                ad.LastViewers = await _userManager.Users.Where(u => contactIds.Contains(u.Id)).Select(u => (ViewApplicationUser)u).ToListAsync();
            }
            else
            {
                ad.LastViewers = new List<ViewApplicationUser> { User };
            }

            return Ok(ad);
        }

        [HttpPost]
        public async Task<ActionResult<AdModel>> Create(CreateAdRequestViewModel createAdRequestViewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ad = (AdModel)createAdRequestViewModel.Ad;
            var applicationUser = (ViewApplicationUser)User;

            if (createAdRequestViewModel.Price != null)
            {
                ad.OrderNumber = string.Concat(DateTime.UtcNow.ToString("yyyMMddHHmmss"), rdm.Next(100000, 999999));

                Subscription subscription = null;
                SubscriptionPack pack = null;

                int delayToDisplay = 0;
                int displayDuration = 0;

                if (applicationUser.IsAdmin || applicationUser.IsManager || applicationUser.IsUnlimited || applicationUser.IsUltimate)
                {
                    delayToDisplay = 0;
                    ad.IsValid = true;
                    ad.ValidateDate = DateTime.UtcNow;
                    displayDuration = 15;
                }
                else
                {
                    if (!applicationUser.HasSubscriptionValid)
                    {
                        delayToDisplay = 2;
                        displayDuration = 2;
                    }
                    else
                    {
                        delayToDisplay = 0;
                        displayDuration = 5;
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

            await _adsUnitOfWork.CategoryAdRepository.Add(createAdRequestViewModel.Ad.ListIdCategory, ad.Id);
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
        public async Task<IActionResult> Update(AdViewModel ad)
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

            await functionalUnitOfWork.AdRepository.Update(model);

            await _adsUnitOfWork.CategoryAdRepository.Add(ad.ListIdCategory, ad.Id);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await functionalUnitOfWork.AdRepository.Delete(id);
            _adsUnitOfWork.CategoryAdRepository.RemoveListCategAd(id);
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
            if (((ViewApplicationUser)User).IsAdmin == false)
                return BadRequest();

            await functionalUnitOfWork.AdRepository.Update(adModel);
            return Ok();
        }

        [HttpGet("evolution/{id}")]
        public async Task<ActionResult<IList<StatisticDatePoint>>> GetEvolution(string id)
        {
            if (User.Identity.IsAuthenticated == false)
                return BadRequest();

            var userId = User.NameIdentifier();
            var eventTrackings = (await _eventTrackingService.Where(e => e.Category == "ViewAd" && e.Key == id)).ToList();

            var groups = eventTrackings.GroupBy(info => info.CreatedDate.Date)
                    .Select(group => new
                    {
                        Date = group.Key,
                        Count = group.Count()
                    }).OrderBy(x => x.Date);

            var result = groups.Select(g => new StatisticDatePoint { Date = g.Date, Value = g.Count }).ToArray();

            return Ok(result);
        }

        [HttpGet("repartition/{id}")]
        public async Task<ActionResult<IList<StatisticDatePoint>>> GetRepartition(string id)
        {
            if (User.Identity.IsAuthenticated == false)
                return BadRequest();

            var userId = User.NameIdentifier();
            var socialNetworks = await functionalUnitOfWork.SocialNetworkRepository.GetSocialNetworkLinks(userId);

            var eventTrackings = (await _eventTrackingService.Where(e => e.Category == "ViewAd" && e.Key == id)).ToList();

            var groups = eventTrackings.GroupBy(info => info.CreatedDate.Date)
                    .Select(group => new
                    {
                        Date = group.Key,
                        Count = group.Count()
                    }).OrderBy(x => x.Date);

            var result = groups.Select(g => new StatisticDatePoint { Date = g.Date, Value = g.Count }).ToArray();

            List<StatisticKeyPoint> repartitions = new List<StatisticKeyPoint>();
            repartitions.AddRange(result.Select(r => new StatisticKeyPoint { Date = r.Date, Key = "Total", Value = r.Value }));

            var dates = repartitions.Where(r => r.Key == "Total").Select(r => r.Date).ToArray();

            foreach (var socialNetwork in socialNetworks)
            {
                var category = $"ViewAdFromSocial#{socialNetwork.Name.ToLower()}";

                var eventTrackingsSocialNetwork = await _eventTrackingService.Where(c => c.Category == category && c.Key == userId);

                if (eventTrackingsSocialNetwork.Any() == false)
                    continue;

                var geventTrackingSocialNetwork = eventTrackingsSocialNetwork.GroupBy(info => info.CreatedDate.Date)
                        .Select(group => new
                        {
                            Date = group.Key,
                            Count = group.Count()
                        }).OrderBy(x => x.Date);

                var resultSocialNetwork = groups.Select(g => new StatisticDatePoint { Date = g.Date, Value = g.Count }).ToArray();

                foreach (var date in dates)
                {
                    repartitions.Add(new StatisticKeyPoint
                    {
                        Key = socialNetwork.Name,
                        Date = date,
                        Value = resultSocialNetwork.Where(r => r.Date == date).Sum(r => r.Value)
                    });
                }
            }

            foreach (var date in dates)
            {
                var total = repartitions.First(d => d.Date == date).Value;
                var other = repartitions.Where(d => d.Date == date && d.Key != "Total").Sum(d => d.Value);
                var direct = total - other;
                repartitions.Add(new StatisticKeyPoint { Date = date, Key = "Direct", Value = direct });
            }

            return Ok(repartitions);
        }

    }
}
