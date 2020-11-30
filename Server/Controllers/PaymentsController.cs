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

        public PaymentsController(FunctionalUnitOfWork context, IConfiguration configuration)
        {
            _functionalUnitOfWork = context;
            _configuration = configuration;
        }

        [HttpGet("create-payment-ad/{AdId}/{OrderNumber}")]
        public async Task<ActionResult<AdModel>> CreatePaymentAd(Guid AdId, string OrderNumber)
        {
            var payment = await _functionalUnitOfWork.PaymentRepository.Add(new PaymentModel
            {
                OrderNumber = OrderNumber,
                AdId = AdId,
                UserId = User.NameIdentifier(),
                PaymentMethod = "CryptoCoin",
            });

            if (payment != null)
                return Ok();
            else
                return BadRequest();
        }
        [HttpGet("create-payment-subscribe/{SubscriptionId}/{OrderNumber}")]
        public async Task<ActionResult<AdModel>> Create(Guid SubscriptionId, string OrderNumber)
        {
            var payment = await _functionalUnitOfWork.PaymentRepository.Add(new PaymentModel
            {
                OrderNumber = OrderNumber,
                SubscriptionId = SubscriptionId,
                UserId = User.NameIdentifier(),
                PaymentMethod = "CryptoCoin",
            });

            if (payment != null)
                return Ok();
            else
                return BadRequest();
        }

        [HttpGet("GetInvoice/{OrderNumber}")]
        public async Task<ActionResult<AdModel>> GetInvoice(string orderNumber)
        {
            var ad = await _functionalUnitOfWork.AdRepository.FirstOrDefault(a => a.OrderNumber == orderNumber && !a.IsValid);
            if (ad == null)
                return BadRequest();
            ad.AdPrice = await _functionalUnitOfWork.AdPriceRepository.FirstOrDefault(x => x.Id == ad.AdPriceId);
            if (ad.AdPrice == null)
                return BadRequest();

            ad.OwnerId = User.NameIdentifier();
            _functionalUnitOfWork.SaveChanges();

            return Ok(ad);
        }
    }
}
