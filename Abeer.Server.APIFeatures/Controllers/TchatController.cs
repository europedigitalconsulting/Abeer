using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abeer.Shared;
using Microsoft.AspNetCore.Authorization;
using Abeer.Data.UnitOfworks;
using System;
using Abeer.Services;
using System.Collections.Generic;
using Abeer.Shared.ViewModels;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using static Abeer.Services.TemplateRenderManager;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TchatController : ControllerBase
    {
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEmailSenderService _emailSender;
        private readonly IConfiguration _configuration;

        public TchatController(FunctionalUnitOfWork functionalUnitOfWork, UserManager<ApplicationUser> userManager, IWebHostEnvironment env, IServiceProvider serviceProvider, IEmailSenderService emailSender, IConfiguration configuration)
        {
            _functionalUnitOfWork = functionalUnitOfWork;
            _userManager = userManager;
            _env = env;
            _serviceProvider = serviceProvider;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        [HttpGet("{contactId}")]
        public async Task<ActionResult<List<Message>>> Getsync(Guid contactId)
        {
            var ListMessage = await _functionalUnitOfWork.MessageRepository.GetMessages(Guid.Parse(User.NameIdentifier()), contactId); 
            foreach (var item in ListMessage.Where(x => x.DateReceived == null).ToList())
            {
                item.DateReceived = DateTime.Now;
                await _functionalUnitOfWork.MessageRepository.Update(item);
            }
            
            return Ok(ListMessage);
        }
        [HttpGet("GetMessageUnread")]
        public async Task<ActionResult<List<string>>> GetMessageUnread()
        {
            var listMsg = await _functionalUnitOfWork.MessageRepository.GetMessageUnread(Guid.Parse(User.NameIdentifier()));
            var result = listMsg.Select(x => x.UserIdFrom).Distinct();

            return Ok(result);
        }
        [HttpPut]
        public async Task<IActionResult> PutAsync(Message model)
        {
            var message = await _functionalUnitOfWork.MessageRepository.FirstOrDefault(x => x.Id == model.Id);

            if (message == null)
                return BadRequest();

            message.DateReceived = DateTime.Now; 

            await _functionalUnitOfWork.MessageRepository.Update(message);
            return Ok(message);
        }
        
        [HttpPost]
        public async Task<IActionResult> PostAsync(TchatViewModel model)
        {
            var contact = await _functionalUnitOfWork.ContactRepository.GetContact(model.ContactId, User.NameIdentifier());

            if (contact == null)
                return BadRequest();

            Message msg = new Message
            {
                Id = Guid.NewGuid(),
                UserIdFrom = Guid.Parse(User.NameIdentifier()),
                UserIdTo = Guid.Parse(model.ContactId),
                DateSent = DateTime.Now,
                Text = model.Text,
            };

            msg = await _functionalUnitOfWork.MessageRepository.Add(msg);

            var userFrom = await _userManager.FindByIdAsync(msg.UserIdFrom.ToString());
            var userTo = await _userManager.FindByIdAsync(model.ContactId.ToString());

            await SendMessageEmail(userFrom, userTo);

            return Ok(msg);
        }

        private async Task SendMessageEmail(ApplicationUser userFrom, ApplicationUser userTo)
        {
            var code = "";

            if (!string.IsNullOrEmpty(userFrom.PinDigit))
                code = userFrom.PinDigit;
            else
                code = userFrom.Id;

            var callbackUrl = $"{Request.Scheme}://{Request.Host}/viewProfile/{code}";

            var frontWebSite = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var logoUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/assets/img/logo_full.png");
            var unSubscribeUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/Account/UnSubscribe");
            var login = $"{userFrom.DisplayName}";

            var parameters = new Dictionary<string, string>()
                        {
                            {"login", login },
                            {"frontWebSite", frontWebSite },
                            {"logoUrl", logoUrl },
                            {"unSubscribeUrl", unSubscribeUrl },
                            {"callbackUrl", callbackUrl },
                            {"userFrom", userFrom.DisplayName }
                        };

            var html = GenerateHtmlTemplate(_serviceProvider, _env.WebRootPath, EmailTemplateEnum.MessageReceived, parameters);
            await _emailSender.SendEmailAsync(userTo.Email, $"{userFrom.DisplayName} vous a envoyé un message sur meetag.co", html);
        }
    }
}
