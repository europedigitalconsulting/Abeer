using Abeer.Data.UnitOfworks;
using Abeer.Services;
using Abeer.Shared;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abeer.Server.APIFeatures.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(NotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> List(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                if (User.Identity.IsAuthenticated)
                {
                    userId = User.NameIdentifier();
                }
            }

            var notifications = await _notificationService.GetNotifications(userId, false);
            return Ok(notifications);
        }

        [HttpPost]
        public async Task<ActionResult> AddNotification(Notification notification)
        {
            if (!User.Identity.IsAuthenticated)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _notificationService.Create(notification);
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> UpdateNotification(Notification notification)
        {
            if (!User.Identity.IsAuthenticated)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _notificationService.Update(notification);
            return Ok();
        }
    }
}
