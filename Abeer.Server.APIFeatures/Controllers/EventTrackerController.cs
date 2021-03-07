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
    public class EventTrackerController : ControllerBase
    {
        private readonly EventTrackingService _EventTrackingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventTrackerController(EventTrackingService eventTrackingService, UserManager<ApplicationUser> userManager)
        {
            _EventTrackingService = eventTrackingService;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet("byUser/{userId}")]
        public async Task<ActionResult<IEnumerable<EventTrackingItem>>> List(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                if (User.Identity.IsAuthenticated)
                {
                    userId = User.NameIdentifier();
                }
            }

            var EventTrackingItems = await _EventTrackingService.GetEventTrackingItems(userId);
            return Ok(EventTrackingItems);
        }

        [AllowAnonymous]
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<EventTrackingItem>>> List()
        {
            var EventTrackingItems = await _EventTrackingService.GetEventTrackingItems();
            return Ok(EventTrackingItems);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> AddEventTrackingItem(EventTrackingItem EventTrackingItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _EventTrackingService.Create(EventTrackingItem);
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> UpdateEventTrackingItem(EventTrackingItem EventTrackingItem)
        {
            if (!User.Identity.IsAuthenticated)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _EventTrackingService.Update(EventTrackingItem);
            return Ok();
        }
    }
}
