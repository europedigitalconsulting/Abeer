using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abeer.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Abeer.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Abeer.Shared.ViewModels;
using Abeer.Data.UnitOfworks;
using System.Collections.Generic;
using System.Linq;
using Abeer.Shared.Functional;
using System;

namespace Abeer.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SubPackController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;

        public SubPackController(UserManager<ApplicationUser> userManager, FunctionalUnitOfWork functionalUnitOfWork)
        {
            _userManager = userManager;
            _functionalUnitOfWork = functionalUnitOfWork;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _functionalUnitOfWork.SubscriptionPackRepository.Where(x => x.Enable);
            return Ok(list);
        }
        [HttpGet("Get/{SubscriptionId}")]
        public async Task<IActionResult> Get(Guid SubscriptionId)
        {
            var result = await _functionalUnitOfWork.SubscriptionPackRepository.FirstOrDefault(x => x.Id == SubscriptionId);
            return Ok(result);
        }
        [HttpPost("Select")]
        public async Task<IActionResult> Select(SubscriptionPack subscriptionPack)
        {
            var user = await _userManager.FindByNameAsync(User.UserName());
            if (user == null)
                return BadRequest();

            SubscriptionHistory newSub = new SubscriptionHistory();
            newSub.Enable = true;
            newSub.Created = DateTime.Now;
            newSub.EndSubscription = DateTime.MaxValue;
            newSub.SubscriptionPackId = subscriptionPack.Id;
            newSub.UserId = Guid.Parse(user.Id);

            await _functionalUnitOfWork.SubscriptionHistoryRepository.AddSubscriptionHistory(newSub);
            return Ok();
        }
    }
}
