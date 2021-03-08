using Abeer.Data.UnitOfworks;
using Abeer.Shared;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Server.APIFeatures.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubscriptionController(IConfiguration configuration, FunctionalUnitOfWork functionalUnitOfWork, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _functionalUnitOfWork = functionalUnitOfWork;
            _userManager = userManager;
        }

        [HttpGet("{subscriptionId}")]
        public async Task<ActionResult<Subscription>> GetSubscription(Guid subscriptionId)
        {
            var subscription = await _functionalUnitOfWork.SubscriptionRepository.FirstOrDefault(s => s.Id == subscriptionId);
            return Ok(subscription);
        }
    }
}
