using System.Collections.Generic;
using System.Threading.Tasks;
using Abeer.Shared;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class ViewProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ViewProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<ViewApplicationUser>> Get(string id)
        {
            var user = await _userManager.FindByIdAsync(id) ?? await _userManager.FindByEmailAsync(id);
            return Ok(new ViewApplicationUser
            {
                City = user.City,
                Email = user.Email, PhoneNumber = user.PhoneNumber, Id = user.Id,
                SocialNetworkConnected = user.SocialNetworkConnected ?? new List<SocialNetwork>(),
                CustomLinks = user.CustomLinks ?? new List<CustomLink>(),
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
                Title = user.Title
            });
        }
    }
}
