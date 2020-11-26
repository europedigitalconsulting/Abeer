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
    public class SubscriptionPackController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;

        public SubscriptionPackController(UserManager<ApplicationUser> userManager, FunctionalUnitOfWork functionalUnitOfWork)
        {
            _userManager = userManager;
            _functionalUnitOfWork = functionalUnitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<SubscriptionPack>> GetAll([FromQuery] string userId)
        {
            var list = await _functionalUnitOfWork.SubscriptionPackRepository.Where(x => x.StartDate < DateTime.Now && x.EndDate > DateTime.Now);
            return Ok(list);
        }
    }
}
