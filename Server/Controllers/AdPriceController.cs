using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Abeer.Shared.Functional;
using Abeer.Data.UnitOfworks;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdPriceController : ControllerBase
    {
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        private readonly IConfiguration _configuration;

        public AdPriceController(FunctionalUnitOfWork functionalUnitOfWork, IConfiguration configuration)
        {
            _functionalUnitOfWork = functionalUnitOfWork;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdPrice>>> List()
        {
            var data = await _functionalUnitOfWork.AdPriceRepository.All();
            return Ok(data);
        }
        [AllowAnonymous]
        [HttpGet("GetFeature")]
        public async Task<ActionResult<AdPaymentOption>> GetFeature()
        {
            return await Task.Run(() =>
            {
                var adPaymentOption = new AdPaymentOption
                {
                    EnableCrypto = !string.IsNullOrEmpty(_configuration["Service:CryptoPayment:Enable"]) && bool.Parse(_configuration["Service:CryptoPayment:Enable"]),
                    EnableBankCard = !string.IsNullOrEmpty(_configuration["Service:CryptoPayment:Enable"]) && bool.Parse(_configuration["Service:CryptoPayment:Enable"])
                };

                return Ok(adPaymentOption);
            });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(AdPrice adPrice)
        {
            var entity = await _functionalUnitOfWork.AdPriceRepository.Add(adPrice);
            return Ok(entity);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update(AdPrice adPrice)
        {
            await _functionalUnitOfWork.AdPriceRepository.Update(adPrice);
            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _functionalUnitOfWork.AdPriceRepository.Delete(id);
            return Ok();
        }
    }
}
