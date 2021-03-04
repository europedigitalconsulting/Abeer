using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Abeer.Shared.Functional;
using Abeer.Data.UnitOfworks;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration;
using Abeer.Shared;
using System.Linq;

namespace Abeer.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        private readonly IConfiguration _configuration;

        public CountriesController(FunctionalUnitOfWork functionalUnitOfWork, IConfiguration configuration)
        {
            _functionalUnitOfWork = functionalUnitOfWork;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Country>>> Get()
        {
            List<Country> countries = await _functionalUnitOfWork.CountriesRepository.GetCountries("fr");

            return Ok(countries.OrderBy(x => x.Name));
        }
    }
}
