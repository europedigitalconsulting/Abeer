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
using Newtonsoft.Json;
using Abeer.Data.UnitOfworks;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Authorization;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaypalController : ControllerBase
    {
        private readonly ILogger<PaypalController> _logger;
        private readonly IConfiguration _configuration;
        private readonly FunctionalUnitOfWork _functionalUnitOfWork; 

        public PaypalController(ILogger<PaypalController> logger, IConfiguration configuration, FunctionalUnitOfWork functionalUnitOfWork)
        {
            _logger = logger;
            _configuration = configuration;
            _functionalUnitOfWork = functionalUnitOfWork;
        }

        [HttpGet("CreateAd/{Id}")]
        public async Task<IActionResult> CreateAd(Guid Id)
        {
            var ad = await _functionalUnitOfWork.AdRepository.FirstOrDefault(p => p.Id == Id);

            if (ad == null)
                return NotFound(); 

            PaymentModel paymentModel = new PaymentModel();
            paymentModel.PaymentMethod = "Paypal";
            paymentModel.UserId = ad.OwnerId;
            paymentModel.AdId = ad.Id;

            return await ProcessCreate(paymentModel, ad.Price);
        } 
        [HttpGet("CreateSubscription/{Id}")]
        public async Task<IActionResult> CreateSubscription(Guid Id)
        {
            var subscription = await _functionalUnitOfWork.SubscriptionPackRepository.FirstOrDefault(p => p.Id == Id);

            if (subscription == null)
                return NotFound();

            PaymentModel paymentModel = new PaymentModel();
            paymentModel.PaymentMethod = "Paypal";
            paymentModel.AdId = subscription.Id;
            paymentModel.UserId =  User.NameIdentifier();

            return await ProcessCreate(paymentModel, subscription.Price);
        } 
        private async Task<IActionResult> ProcessCreate(PaymentModel paymentModel, decimal price)
        {
            _logger.LogInformation("Creating payment against Paypal API SDK");
            var payment = await CreatePayment(paymentModel, price);

            _logger.LogInformation($"Created payment successfully : '{payment}' from the Paypal API SDK");

            foreach (var link in payment.Links)
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

            var ad = await _functionalUnitOfWork.AdRepository.FirstOrDefault(p => p.Id == paymentModel.AdId);
            ad.IsValid = true;
            ad.ValidateDate = DateTime.Now;

            _functionalUnitOfWork.SaveChanges();

            _logger.LogInformation($"The paypal controller has a new response '{result}' from the Paypal API");
            return Redirect($"{_configuration["Service:FrontOffice:Url"].TrimEnd('/')}/ConfirmPayment/Success");
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

        private async Task<PayPal.v1.Payments.Payment> CreatePayment(PaymentModel model, decimal value)
        {
            var environment = new SandboxEnvironment(_configuration["Service:Paypal:ClientId"], _configuration["Service:Paypal:ClientSecret"]);
            var client = new PayPalHttpClient(environment);

            var payment = new PayPal.v1.Payments.Payment()
            {
                Intent = "sale",
                Transactions = new List<Transaction>()
                {
                    new Transaction()
                    {
                        Amount = new Amount()
                        {
                            Total = string.Format(CultureInfo.InvariantCulture, "{0:0.00}", value)
                                .Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."),
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

                await _functionalUnitOfWork.PaymentRepository.Add(model); 

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
