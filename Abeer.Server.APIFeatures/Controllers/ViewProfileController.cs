using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Abeer.Data.UnitOfworks;
using Abeer.Services;
using Abeer.Shared;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class ViewProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        private readonly EventTrackingService _eventTrackingService;

        public ViewProfileController(UserManager<ApplicationUser> userManager, FunctionalUnitOfWork functionalUnitOfWork, EventTrackingService eventTrackingService)
        {
            _userManager = userManager;
            _functionalUnitOfWork = functionalUnitOfWork;
            _eventTrackingService = eventTrackingService;
        }

        [AllowAnonymous]
        [HttpGet("{userId}")]
        public async Task<ActionResult<ViewApplicationUser>> Get(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest();

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PinDigit == userId || u.Id == userId || u.UserName == userId);

            if (user == null)
                return NotFound();

            if (Request.Query.Any())
            {
                if (QueryHelpers.ParseQuery(Request.QueryString.Value).TryGetValue("social", out var _social))
                {
                    await _eventTrackingService.Create(User.NameIdentifier(), $"ViewProfileFromSocial#{_social}", userId);
                }
            }

            user.NubmerOfView += 1;
            await _userManager.UpdateAsync(user);

            if(!User.Identity.IsAuthenticated)
                await _eventTrackingService.Create(User.NameIdentifier(), "ViewProfile", userId);
            else
                await _eventTrackingService.Create(null, "ViewProfile", userId);

            var view = (ViewApplicationUser)user;

            view.NumberOfContacts = (await _functionalUnitOfWork.ContactRepository.GetContacts(userId)).Count;
            view.NumberOfAds = (await _functionalUnitOfWork.AdRepository.GetVisibledUser(userId)).Count;

            view.SocialNetworkConnected = await _functionalUnitOfWork
                .SocialNetworkRepository
                .GetSocialNetworkLinks(user.Id) ?? new List<SocialNetwork>();

            view.CustomLinks = await _functionalUnitOfWork
                    .CustomLinkRepository
                    .GetCustomLinkLinks(user.Id) ?? new List<CustomLink>();

            view.PhotoUrl = string.IsNullOrWhiteSpace(user.PhotoUrl) ? user.GravatarUrl() : user.PhotoUrl;

            return Ok(view);
        }
    }
}
