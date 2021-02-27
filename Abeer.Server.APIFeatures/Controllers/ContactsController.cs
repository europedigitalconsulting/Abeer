using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Abeer.Shared;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;
using Abeer.Data.UnitOfworks;
using System;
using Abeer.Services;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Http.Extensions; 
using static Abeer.Services.TemplateRenderManager;
using Microsoft.AspNetCore.Hosting;

namespace Abeer.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FunctionalUnitOfWork _UnitOfWork;
        private readonly IWebHostEnvironment _env;
        private readonly IServiceProvider _serviceProvider;
        private readonly UrlShortner _urlShortner;
        private readonly IEmailSenderService _emailSender;

        public ContactsController(
            UrlShortner urlShortner,
            IWebHostEnvironment env,
            IServiceProvider serviceProvider,
            IEmailSenderService emailSender, FunctionalUnitOfWork onlineWalletUnitOfWork, UserManager<ApplicationUser> userManager)
        {
            _serviceProvider = serviceProvider;
            _UnitOfWork = onlineWalletUnitOfWork;
            _userManager = userManager;
            _emailSender = emailSender;
            _urlShortner = urlShortner;
            _env = env;
        }

        // GET: api/Contacts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ViewContact>>> GetContacts()
        {
            List<ViewContact> viewContacts = new List<ViewContact>();
            var contacts = (await _UnitOfWork.ContactRepository.GetContacts(User.NameIdentifier()))?.ToList();

            if (contacts.Any())
            {
                contacts.ForEach(async (contact) =>
                 {
                     var user = await _userManager.FindByIdAsync(contact.UserId);
                     ViewContact item = new ViewContact(user, contact);

                     user.NubmerOfView += 1;
                     await _userManager.UpdateAsync(user);

                     item.NumberOfView = user.NubmerOfView;
                     item.SocialNetworks = await _UnitOfWork.SocialNetworkRepository.GetSocialNetworkLinks(contact.UserId) ??
                         new List<SocialNetwork>();

                     item.CustomLinks = new List<CustomLink>();
                     viewContacts.Add(item);
                 });
            }

            return viewContacts;
        }

        [HttpGet("add/{id}")]
        public async Task<ActionResult<ImportContactResultViewModel>> Add(string id)
        {
            if (User.NameIdentifier() == id)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var contact = await _UnitOfWork.ContactRepository.FirstOrDefault(c => c.UserId == user.Id && c.OwnerId == User.NameIdentifier());

            if (contact == null)
            {
                var result = await _UnitOfWork.ContactRepository.Add(new Contact
                {
                    OwnerId = User.NameIdentifier(),
                    UserId = user.Id

                });
              //  await SendEmailTemplate(user);
                return Ok();
            }
            return Conflict();
        }
        private async Task SendEmailTemplate(ApplicationUser user)
        {
            var callbackUrl = Url.Action("GetContacts", "Contacts", 
                values: new {userId = user.Id },
                protocol: Request.Scheme);

            var frontWebSite = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var logoUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/assets/img/logo_full.png");
            var unSubscribeUrl = await _urlShortner.CreateUrl(Request.Scheme, Request.Host, UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/Account/UnSubscribe"));
            var login = $"{user.Email}";

            callbackUrl = await _urlShortner.CreateUrl(Request.Scheme, Request.Host, callbackUrl); 

           var parameters = new Dictionary<string, string>()
                        {
                            {"login", login },
                            {"frontWebSite", frontWebSite },
                            {"logoUrl", logoUrl },
                            {"unSubscribeUrl", unSubscribeUrl },
                            {"callbackUrl", callbackUrl }
                        };

            var message = GenerateHtmlTemplate(_serviceProvider, _env.WebRootPath, "email-confirmation", parameters);
            await _emailSender.SendEmailAsync(user.Email, "Add Contact", message);
        }
        [HttpGet("Suggestions")]
        public async Task<ActionResult<IEnumerable<ViewContact>>> GetSuggestion(string term)
        {
            if (string.IsNullOrEmpty(term))
                return BadRequest();

            var users = await Task.Run(() => _userManager?.Users.ToList().Where(c => c.EmailConfirmed &&
                            c.FirstName != null && c.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase)
                          || c.LastName != null && c.LastName.Contains(term, StringComparison.OrdinalIgnoreCase)
                          || c.Description != null && c.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                         c.DisplayName != null && c.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                         c.Email != null && c.Email.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                         c.Title != null && c.Title.Contains(term, StringComparison.OrdinalIgnoreCase)));

            if (users == null)
                return NotFound();

            List<ViewContact> contacts = new List<ViewContact>();
            var allUserContacts = await _UnitOfWork.ContactRepository.GetContacts();
            users = users.Where(u => !allUserContacts.Any(c => u.Id == c.UserId || u.Id == User.NameIdentifier())).ToList();

            foreach (var user in users)
            {
                contacts.Add(new ViewContact(user, null));
            }

            return contacts;
        }

        // GET: api/Contacts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Contact>> GetContact(Guid id)
        {
            var contact = await _UnitOfWork.ContactRepository.GetContact(id);

            if (contact == null)
            {
                return NotFound();
            }

            return contact;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveContactAsync(Guid id)
        {
            await _UnitOfWork.ContactRepository.Delete(id);
            _UnitOfWork.SaveChanges();
            return Ok();
        }

        // PUT: api/Contacts/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContact(Guid id, Contact contact)
        {
            if (id != contact.Id)
            {
                return BadRequest();
            }

            await _UnitOfWork.ContactRepository.Update(contact);

            return NoContent();
        }

        // POST: api/Contacts
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Contact>> PostContact(Contact contact)
        {
            contact.OwnerId = User.NameIdentifier();

            await _UnitOfWork.ContactRepository.Add(contact);
            return CreatedAtAction("GetContact", new { id = contact.Id }, contact);
        }
    }
}
