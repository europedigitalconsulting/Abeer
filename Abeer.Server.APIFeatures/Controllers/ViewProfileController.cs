using System.Collections.Generic;
using System.Threading.Tasks;

using Abeer.Data.UnitOfworks;
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
        [HttpGet("{id}")]
        public async Task<ActionResult<ViewApplicationUser>> Get(string id)
        {
            var user = await _userManager.FindByIdAsync(id) ?? await _userManager.Users.FirstOrDefaultAsync(u => u.PinDigit == id)
                ?? await _userManager.FindByEmailAsync(id);

            if (user == null)
                return NotFound();

            return Ok(new ViewApplicationUser
            {
                City = user.City,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Id = user.Id,
                SocialNetworkConnected = await _functionalUnitOfWork.SocialNetworkRepository.GetSocialNetworkLinks(user.Id) ?? new List<SocialNetwork>(),
                CustomLinks = await _functionalUnitOfWork.CustomLinkRepository.GetCustomLinkLinks(user.Id) ?? new List<CustomLink>(),
                Address = user.Address,
                Country = user.Country,
                Description = user.Description,
                DisplayName = user.DisplayName,
                FirstName = user.FirstName,
                IsAdmin = user.IsAdmin,
                IsManager = user.IsManager,
                IsOnline = user.IsOnline,
                IsOperator = user.IsOperator,
                LastLogin = user.LastLogin,
                LastName = user.LastName,
                Title = user.Title,
                PhotoUrl = user.PhotoUrl
            });
        }
    }
}
