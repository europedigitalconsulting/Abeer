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
    public class CryptocoinController : ControllerBase
    {
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        private readonly IConfiguration _configuration;

        public CryptocoinController(FunctionalUnitOfWork context, IConfiguration configuration)
        {
            _functionalUnitOfWork = context;
            _configuration = configuration;
        }

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

                    var payment = await _functionalUnitOfWork.PaymentRepository.FirstOrDefault(x => x.AdId == ad.Id);
                    payment.IsValidated = true;
                    payment.ValidatedDate = DateTime.Now;
                    payment.PaymentReference = cryptoPaymentInfo.OrderNumber;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

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

    }
}
