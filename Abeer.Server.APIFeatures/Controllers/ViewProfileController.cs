using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Abeer.Data.UnitOfworks;
using Abeer.Services;
using Abeer.Shared;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public ViewProfileController(UserManager<ApplicationUser> userManager, FunctionalUnitOfWork functionalUnitOfWork)
        {
            _userManager = userManager;
            _functionalUnitOfWork = functionalUnitOfWork;
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

            var view = (ViewApplicationUser)user;

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
