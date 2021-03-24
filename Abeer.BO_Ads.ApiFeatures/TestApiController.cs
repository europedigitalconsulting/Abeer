using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abeer.Ads.Data;
using Microsoft.AspNetCore.Mvc;

namespace Abeer.Ads.ApiFeatures
{
    [Route("test/bo/[controller]")]
    [ApiController]
    public class TestApiController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> EchoFromServer()
        {
            return Ok($"Echo from module Ads - Date {DateTime.UtcNow}");
        }

        [HttpGet("data")]
        public async Task<ActionResult<string>> EchoWithData([FromServices] AdsUnitOfWork adsUnitOfWork)
        {
            var watcher = new Stopwatch();
            watcher.Start();

            var families = await adsUnitOfWork.FamiliesRepository.GetAll();
            
            return Ok(
                $"echo from module Ads with data - Date {DateTime.UtcNow} - nb of families {families.Count} getted in {watcher.Elapsed}");
        }
    }
}
