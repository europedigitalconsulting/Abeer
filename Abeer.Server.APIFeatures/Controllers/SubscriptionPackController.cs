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
using Cryptocoin.Payment;

namespace Abeer.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SubPackController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        private readonly IConfiguration _configuration;

        public SubPackController(UserManager<ApplicationUser> userManager, FunctionalUnitOfWork functionalUnitOfWork, IConfiguration configuration)
        {
            _userManager = userManager;
            _functionalUnitOfWork = functionalUnitOfWork;
            _configuration = configuration;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _functionalUnitOfWork.SubscriptionPackRepository.Where(x => x.Enable);
            return Ok(list);
        }

        [HttpGet("Get/{SubscriptionId}")]
        public async Task<IActionResult> Get(Guid SubscriptionId)
        {
            var subPack = await _functionalUnitOfWork.SubscriptionPackRepository.FirstOrDefault(x => x.Id == SubscriptionId);
            return Ok(subPack);
        }
        [HttpGet("GetConfigCrypto")]
        public async Task<IActionResult> GetConfigCrypto()
        {
            CryptoPaymentModel cryptoPaymentViewModel = new CryptoPaymentModel();
            cryptoPaymentViewModel.ClientId = _configuration["Service:CryptoPayment:ClientId"];
            cryptoPaymentViewModel.ClientSecret = _configuration["Service:CryptoPayment:ClientSecret"];
            cryptoPaymentViewModel.DomainApiPayment = _configuration["Service:CryptoPayment:DomainApiPayment"];
            cryptoPaymentViewModel.RedirectSuccessServer = _configuration["Service:CryptoPayment:RedirectSuccessSubServer"];
            cryptoPaymentViewModel.RedirectErrorServer = _configuration["Service:CryptoPayment:RedirectErrorServer"];
            cryptoPaymentViewModel.RedirectSuccess = _configuration["Service:CryptoPayment:RedirectSuccess"];
            cryptoPaymentViewModel.RedirectError = _configuration["Service:CryptoPayment:RedirectError"];
            cryptoPaymentViewModel.EnableCryptoPayment = bool.Parse(_configuration["Service:CryptoPayment:Enable"]);
            cryptoPaymentViewModel.VTA = _configuration["Service:Payment:VTA"];

            return await Task.Run(() => Ok(cryptoPaymentViewModel));
        }
        [HttpPost("Select")]
        public async Task<IActionResult> Select(SubscriptionPack subscriptionPack)
        {
            var user = await _userManager.FindByNameAsync(User.UserName());
            if (user == null)
                return BadRequest();

            SubscriptionHistory newSub = new SubscriptionHistory();
            newSub.Enable = true;
            newSub.Created = DateTime.Now;
            newSub.EndSubscription = DateTime.MaxValue;
            newSub.SubscriptionPackId = subscriptionPack.Id;
            newSub.UserId = Guid.Parse(user.Id);

            await _functionalUnitOfWork.SubscriptionHistoryRepository.Add(newSub);
            return Ok();
        }
    }
}
