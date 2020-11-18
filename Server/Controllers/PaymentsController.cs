using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abeer.Shared;
using Abeer.Data.UnitOfworks;
using Microsoft.AspNetCore.Authorization;
using Abeer.Shared.Technical;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Abeer.Shared.Functional;
using Microsoft.Extensions.DependencyInjection;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public PaymentsController(FunctionalUnitOfWork context, IConfiguration configuration)
        {
            _functionalUnitOfWork = context;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("ProcessingCryptoCoinSuccess")]
        public async Task ProcessingCryptoCoinSuccess(CryptoPaymentInfo cryptoPaymentInfo)
        {
            try
            {
                var client = new HttpClient();
                var result = await client.PostAsJsonAsync($"{_configuration["Service:CryptoPayment:VerifyApiValidationToken"]}", cryptoPaymentInfo);
                if (result.IsSuccessStatusCode)
                {  
                        var ad = await _functionalUnitOfWork.AdRepository.FirstOrDefault(a => a.OrderNumber == cryptoPaymentInfo.OrderNumber && !a.IsValid);
                        ad.IsValid = true;
                        ad.ValidateDate = DateTime.Now;
                        _functionalUnitOfWork.SaveChanges(); 
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        [AllowAnonymous]
        [HttpPost("ProcessingCryptoCoinFailed")]
        public async Task ProcessingCryptoCoinFailed(CryptoPaymentInfo cryptoPaymentInfo)
        {
            try
            {
                var client = new HttpClient();
                var result = await client.PostAsJsonAsync($"{_configuration["Service:CryptoPayment:VerifyApiValidationToken"]}", cryptoPaymentInfo);
                if (result.IsSuccessStatusCode)
                {

                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [Authorize]
        [HttpGet("GetInvoice/{OrderNumber}")]
        public async Task<ActionResult<AdModel>> GetInvoice(string orderNumber)
        {
            var ad = await _functionalUnitOfWork.AdRepository.FirstOrDefault(a => a.OrderNumber == orderNumber && !a.IsValid);
            if (ad == null)
                return BadRequest();
            ad.AdPrice = await _functionalUnitOfWork.AdPriceRepository.FirstOrDefault(x => x.Id == ad.AdPriceId);
            if (ad.AdPrice == null)
                return BadRequest();
            return Ok(ad);
        }
    }
}
