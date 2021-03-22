using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Abeer.Shared;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;
using Abeer.Data.UnitOfworks;
using System;
using Abeer.Shared.ViewModels;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgenciesController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FunctionalUnitOfWork _UnitOfWork;

        private readonly FunctionalUnitOfWork functionalUnitOfWork;

        public AgenciesController()
        {
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Shared.Functional.Agency>>> List()
        {
            var agencies = new List<Shared.Functional.Agency>
            {
                new Shared.Functional.Agency
                {
                    AgencyName = "Tunis", 
                    DisplayName = "Agence Abeer Europe Tunis",
                    Address = "Center Town",
                    Country = "Tunisie",
                    City = "Tunis",
                    Email = "tunis@meetag.co",
                    PhoneNumber = "+961 1 494 970",
                    PhotoUrl = "",
                    MapUrl=""
                }
            };

            return Ok(agencies);
        }
    }
}
