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
using Microsoft.AspNetCore.Identity;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CryptocoinController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        private readonly IConfiguration _configuration;

        public CryptocoinController(UserManager<ApplicationUser> userManager, FunctionalUnitOfWork context, IConfiguration configuration)
        {
            _functionalUnitOfWork = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("ProcessCryptoAdSuccess")]
        public async Task ProcessCryptoAdSuccess(CryptoPaymentInfo cryptoPaymentInfo)
        {
            try
            {
                var client = new HttpClient();
                var result = await client.PostAsJsonAsync($"{_configuration["Service:CryptoPayment:VerifyApiValidationToken"]}", cryptoPaymentInfo);
                if (result.IsSuccessStatusCode)
                {
                    var payment = await _functionalUnitOfWork.PaymentRepository.FirstOrDefault(a => a.OrderNumber == cryptoPaymentInfo.OrderNumber);

                    payment.IsValidated = true;
                    payment.ValidatedDate = DateTime.Now;
                    payment.OrderNumber = cryptoPaymentInfo.OrderNumber;
                    payment.TokenId = cryptoPaymentInfo.Token;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost("ProcessCryptoSubSuccess")]
        public async Task ProcessCryptoSubSuccess(CryptoPaymentInfo cryptoPaymentInfo)
        {
            try
            {
                var client = new HttpClient();
                var result = await client.PostAsJsonAsync($"{_configuration["Service:CryptoPayment:VerifyApiValidationToken"]}", cryptoPaymentInfo);
                if (result.IsSuccessStatusCode)
                {
                    var payment = await _functionalUnitOfWork.PaymentRepository.FirstOrDefault(a => a.OrderNumber == cryptoPaymentInfo.OrderNumber);
                    var subscription = await _functionalUnitOfWork.SubscriptionPackRepository.FirstOrDefault(x => x.Id == payment.SubscriptionId.Value);

                    payment.IsValidated = true;
                    payment.ValidatedDate = DateTime.Now;
                    payment.OrderNumber = cryptoPaymentInfo.OrderNumber;
                    payment.TokenId = cryptoPaymentInfo.Token;

                    SubscriptionHistory subHisto = new SubscriptionHistory();
                    subHisto.Created = DateTime.Now;
                    subHisto.EndSubscription = DateTime.Now.AddMonths(subscription.Duration);
                    subHisto.Enable = true;
                    subHisto.UserId = Guid.Parse(payment.UserId);
                    subHisto.SubscriptionPackId = payment.SubscriptionId.Value;
                    await _functionalUnitOfWork.SubscriptionHistoryRepository.Add(subHisto);

                    var user = await _userManager.FindByIdAsync(payment.UserId);
                    user.SubscriptionStartDate = user.SubscriptionStartDate.HasValue ? user.SubscriptionStartDate : subHisto.Created;
                    if (user.SubscriptionEndDate.HasValue)
                    {
                        if (user.SubscriptionEndDate.Value < DateTime.Now) 
                            user.SubscriptionEndDate = DateTime.Now.AddMonths(subscription.Duration); 
                        else
                            user.SubscriptionEndDate = user.SubscriptionEndDate.Value.AddMonths(subscription.Duration);
                    }
                    else
                        user.SubscriptionEndDate = subHisto.EndSubscription;  

                    await _userManager.UpdateAsync(user); 
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost("ProcessCryptoFailed")]
        public async Task ProcessCryptoFailed(CryptoPaymentInfo cryptoPaymentInfo)
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
