using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;

using PayPal.v1.Payments;
using PayPal.Core;
using System.Collections.Generic;
using Transaction = PayPal.v1.Payments.Transaction;
using BraintreeHttp;
using System.Linq;
using Abeer.Data.UnitOfworks;
using Abeer.Services;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Identity;
using Abeer.Shared;
using Microsoft.AspNetCore.Hosting;
using static Abeer.Services.TemplateRenderManager;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaypalController : ControllerBase
    {
        private readonly ILogger<PaypalController> _logger;
        private readonly IConfiguration _configuration;
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UrlShortner _urlShortner;
        private readonly IEmailSenderService _emailSenderService;
        private readonly IWebHostEnvironment _env;
        private readonly IServiceProvider _serviceProvider;

        private readonly Random rnd = new Random();

        public PaypalController(ILogger<PaypalController> logger, IConfiguration configuration, FunctionalUnitOfWork functionalUnitOfWork, UserManager<ApplicationUser> userManager, UrlShortner urlShortner, IEmailSenderService emailSenderService, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _functionalUnitOfWork = functionalUnitOfWork;
            _userManager = userManager;
            _urlShortner = urlShortner;
            _emailSenderService = emailSenderService;
            _env = env;
            _serviceProvider = serviceProvider;
        }

        /*
        [HttpGet("CreateAd/{Id}")]
        public async Task<IActionResult> CreateAd(Guid Id)
        {
            var ad = await _functionalUnitOfWork.AdRepository.FirstOrDefault(p => p.Id == Id);

            if (ad == null)
                return NotFound();

            PaymentModel paymentModel = new PaymentModel
            {
                PaymentMethod = "Paypal",
                UserId = ad.OwnerId,
                Reference = ad.Id,
                PaymentType = "Ad"
            };

            return await ProcessCreate(paymentModel, ad.Price);
        }
        */

        [HttpPost("CreateSubscription/{orderNumber}")]
        public async Task<IActionResult> CreateSubscription(Subscription subscription, string orderNumber)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            subscription.Start = User.SubscriptionEnd().GetValueOrDefault(DateTime.UtcNow);
            subscription.End = subscription.Start.AddMonths(subscription.SubscriptionPack.Duration);
            
            var pack = subscription.SubscriptionPack;

            var newOne = new Subscription
            {
                Id = subscription.Id,
                CreateDate = subscription.CreateDate,
                Start = subscription.Start,
                SubscriptionPackId = subscription.SubscriptionPackId,
                UserId = subscription.UserId,
                End = subscription.End
            };

            subscription = await _functionalUnitOfWork.SubscriptionRepository.Add(newOne);

            var tax = decimal.Parse(_configuration["Service:Payment:VTA"]);

            var subTotal = pack.Price;
            var totalTax = (subTotal * tax) / 100.00M;
            var totalTTC = totalTax + subTotal;

            PaymentModel paymentModel = new PaymentModel
            {
                PaymentMethod = "Paypal",
                Reference = newOne.Id,
                UserId = newOne.UserId,
                PaymentType = "Subscription",
                OrderNumber = orderNumber,
                SubscriptionId = subscription.Id,
                SubTotal = subTotal, 
                TotalTax = totalTax, 
                TotalTTc = totalTTC
            };

            await _functionalUnitOfWork.PaymentRepository.Add(paymentModel);
            subscription.PaymentId = paymentModel.Id;

            await _functionalUnitOfWork.SubscriptionRepository.Update(subscription);

            return Ok(paymentModel);
        }

        [HttpGet("pay/{paymentId}")]
        public async Task<IActionResult> Pay(Guid paymentId)
        {
            var payment = await _functionalUnitOfWork.PaymentRepository.FirstOrDefault(p => p.Id == paymentId);
            _logger.LogInformation("Creating payment against Paypal API SDK");

            var ticket = await ProcessPayment(payment);

            if(ticket.Payer.PayerInfo != null)
                payment.PayerID = ticket.Payer.PayerInfo.PayerId;

            payment.NoteToPayer = ticket.NoteToPayer;
            payment.PaymentMethod = ticket.Payer.PaymentMethod;

            await _functionalUnitOfWork.PaymentRepository.Update(payment);

            _logger.LogInformation($"Created payment successfully : '{payment}' from the Paypal API SDK");

            foreach (var link in ticket.Links)
            {
                if (link.Rel.Equals("approval_url"))
                {
                    _logger.LogInformation($"Found the approval URL: '{link.Href}' from response");
                    return Redirect(link.Href);
                }
            }

            return NotFound();
        }

        [HttpGet("success")]
        public async Task<IActionResult> ExecuteSuccess(string paymentID, string token, string payerID, [FromServices] IServiceProvider serviceProvider)
        {
            _logger.LogInformation("Executing the payment against the PAYPAL SDK");

            var paymentModel = await _functionalUnitOfWork.PaymentRepository.FirstOrDefault(p => p.PaymentReference == paymentID);

            PayPal.v1.Payments.Payment result = await ExecutePayment(payerID, paymentID);

            paymentModel.PayerID = payerID;
            paymentModel.TokenId = token;
            paymentModel.IsValidated = true;
            paymentModel.ValidatedDate = DateTime.UtcNow;

            if (paymentModel.PaymentType == "Subscription")
            {
                var user = await _userManager.FindByIdAsync(paymentModel.UserId);

                var subscription = await _functionalUnitOfWork.SubscriptionRepository.FirstOrDefault(s => s.Id == paymentModel.Reference);
                subscription.Enable = true;
                await _functionalUnitOfWork.SubscriptionRepository.Update(subscription);

                var pack = await _functionalUnitOfWork.SubscriptionPackRepository.FirstOrDefault(p => p.Id == subscription.SubscriptionPackId);
                user.SubscriptionStartDate = user.SubscriptionEndDate;
                user.SubscriptionEndDate = user.SubscriptionStartDate.Value.AddMonths(pack.Duration);
                await _userManager.UpdateAsync(user);

                _logger.LogInformation($"The paypal controller has a new response '{result}' from the Paypal API");

                await SendEmailTemplate(user, subscription.Id, "subscription", "subscription");

                return Redirect($"{_configuration["Service:FrontOffice:Url"].TrimEnd('/')}/ConfirmPayment/Success/{subscription.Id}");
            }
            else if (paymentModel.PaymentType == "Ad")
            {
                var ad = await _functionalUnitOfWork.AdRepository.FirstOrDefault(p => p.Id == paymentModel.Reference);
                ad.IsValid = true;
                ad.ValidateDate = DateTime.Now;
                await _functionalUnitOfWork.AdRepository.Update(ad);
                _logger.LogInformation($"The paypal controller has a new response '{result}' from the Paypal API");
                return Redirect($"{_configuration["Service:FrontOffice:Url"].TrimEnd('/')}/ConfirmPayment/Success");
            }

            return BadRequest();
        }

        private async Task SendEmailTemplate(ApplicationUser user, Guid subscriptionId, string emailTemplate, string emailSubject)
        {
            var longUrl = $"{_configuration["Service:FrontOffice:Url"].TrimEnd('/')}/ConfirmPayment/Success/{subscriptionId}";

            var frontWebSite = _configuration["Service:FrontOffice:Url"];
            var logoUrl = $"{_configuration["Service:FrontOffice:Url"]}/assets/img/logo_full.png";
            var login = $"{user.DisplayName}";

            var code = GenerateCode();
            var shortedUrl = $"{_configuration["Service:FrontOffice:Url"].TrimEnd('/')}/shortned/{code}";

            var callbackUrl = await _urlShortner.CreateUrl(false, false, shortedUrl, longUrl, null, code);

            var parameters = new Dictionary<string, string>()
                        {
                            {"login", login },
                            {"frontWebSite", frontWebSite },
                            {"logoUrl", logoUrl },
                            {"callbackUrl", callbackUrl }
                        };

            var message = GenerateHtmlTemplate(_serviceProvider, _env.WebRootPath, emailTemplate, parameters);
            await _emailSenderService.SendEmailAsync(user.Email, emailSubject, message);
        }

        private static string GenerateCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];

            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }


        [HttpGet("cancel")]
        public async Task<IActionResult> CancelPayment()
        {
            return Redirect($"{_configuration["Service:FrontOffice:Url"].TrimEnd('/')}");
        }

        private async Task<PayPal.v1.Payments.Payment> ExecutePayment(string payerId, string paymentId)
        {
            var environment = new SandboxEnvironment(_configuration["Service:Paypal:ClientId"], _configuration["Service:Paypal:ClientSecret"]);
            var client = new PayPalHttpClient(environment);
            var paymentExecution = new PaymentExecution { PayerId = payerId };
            var paymentExecutionRequest = new PaymentExecuteRequest(paymentId).RequestBody(paymentExecution);
            var response = await Task.Run(() => client.Execute<PaymentExecuteRequest>(paymentExecutionRequest));
            return response.Result<PayPal.v1.Payments.Payment>();
        }

        private async Task<PayPal.v1.Payments.Payment> ProcessPayment(PaymentModel model)
        {
            var mode = _configuration["Service:Paypal:Mode"];
            PayPalHttpClient client = null;

            switch (mode)
            {
                case "production":
                    {
                        var environment = new LiveEnvironment(_configuration["Service:Paypal:ClientId"], _configuration["Service:Paypal:ClientSecret"]);
                        client = new PayPalHttpClient(environment);
                        break;
                    }
                case "sandbox":
                    {
                        var environment = new SandboxEnvironment(_configuration["Service:Paypal:ClientId"], _configuration["Service:Paypal:ClientSecret"]);
                        client = new PayPalHttpClient(environment);
                        break;
                    }
            }

            var payment = new PayPal.v1.Payments.Payment()
            {
                Intent = "sale",
                Transactions = new List<Transaction>()
                {
                    new Transaction()
                    {
                        Amount = new Amount()
                        {
                            Total = string.Format(CultureInfo.InvariantCulture, "{0:0.00}", model.TotalTTc)
                                .Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), Details =new AmountDetails
                                {
                                    Subtotal = string.Format(CultureInfo.InvariantCulture, "{0:0.00}", model.SubTotal),
                                    Tax =string.Format(CultureInfo.InvariantCulture, "{0:0.00}", model.TotalTax)
                                },
                            Currency = "EUR"
                        }
                    }
                },
                RedirectUrls = new RedirectUrls()
                {
                    CancelUrl = $"{_configuration["Service:FrontOffice:Url"].TrimEnd('/')}/api/paypal/cancel",
                    ReturnUrl = $"{_configuration["Service:FrontOffice:Url"].TrimEnd('/')}/api/paypal/success"
                },
                Payer = new Payer()
                {
                    PaymentMethod = "paypal"
                }
            };

            PaymentCreateRequest request = new PaymentCreateRequest();
            request.RequestBody(payment);

            try
            {
                HttpResponse response = await client.Execute(request);

                var statusCode = response.StatusCode;

                PayPal.v1.Payments.Payment result = response.Result<PayPal.v1.Payments.Payment>();
                model.PaymentReference = result.Id;

                await _functionalUnitOfWork.PaymentRepository.Update(model);

                return result;
            }
            catch (HttpException httpException)
            {
                var statusCode = httpException.StatusCode;
                var debugId = httpException.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();

                throw;
            }
        }
    }
}
