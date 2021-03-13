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
using Abeer.Shared.ReferentielTable;
using Abeer.Shared.Technical;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Abeer.Shared.Functional;

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
        private readonly IConfiguration _configuration;
        private readonly NotificationService _notificationService;
        public ContactsController(
            UrlShortner urlShortner, NotificationService notificationService,
            IWebHostEnvironment env, IConfiguration configuration,
            IServiceProvider serviceProvider,
            IEmailSenderService emailSender, FunctionalUnitOfWork onlineWalletUnitOfWork, UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _serviceProvider = serviceProvider;
            _UnitOfWork = onlineWalletUnitOfWork;
            _userManager = userManager;
            _emailSender = emailSender;
            _urlShortner = urlShortner;
            _env = env;
            _configuration = configuration;
        }

        // GET: api/Contacts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ViewContact>>> GetContacts()
        {
            List<ViewContact> viewContacts = new List<ViewContact>();

            var user = await _userManager.FindByIdAsync(User.NameIdentifier());

            var contacts = (await _UnitOfWork.ContactRepository.GetContacts(user.Id))?.ToList();

            if (contacts.Any())
            {
                contacts.ForEach(async (contact) =>
               {
                   var uContact = await _userManager.FindByIdAsync(contact.UserId);
                   ViewContact item = new(user, uContact, contact);

                   user.NubmerOfView += 1;
                   await _userManager.UpdateAsync(user);

                   item.Contact.NumberOfView = user.NubmerOfView;
                   item.Contact.SocialNetworkConnected = await _UnitOfWork.SocialNetworkRepository.GetSocialNetworkLinks(contact.UserId) ??
                       new List<SocialNetwork>();
                   
                   item.Contact.CustomLinks = await _UnitOfWork.CustomLinkRepository.GetCustomLinkLinks(contact.UserId) ?? new List<CustomLink>();
                   viewContacts.Add(item);
               });
            }

            return Ok(viewContacts);
        }

        [HttpGet("add/{contactId}")]
        public async Task<ActionResult<ViewContact>> AddInvitation(string contactId)
        {
            if (string.IsNullOrEmpty(contactId))
                return BadRequest();

            if (User.NameIdentifier() == contactId)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(User.NameIdentifier());
            var userContact = await _userManager.FindByIdAsync(contactId);

            if (userContact == null)
                return NotFound();

            var contact = await _UnitOfWork.ContactRepository.FirstOrDefault(c => c.UserId == userContact.Id && c.OwnerId == User.NameIdentifier());

            if (contact == null)
            {
                var result = await _UnitOfWork.ContactRepository.Add(new Contact
                {
                    OwnerId = User.NameIdentifier(),
                    UserId = userContact.Id,
                    UserAccepted = EnumUserAccepted.PENDING
                });

                var invitation = new Invitation
                {
                    OwnedId = User.NameIdentifier(),
                    ContactId = result.Id.ToString(),
                    CreatedDate = DateTime.UtcNow,
                    InvitationStat = (int)EnumUserAccepted.PENDING
                };

                await _UnitOfWork.InvitationRepository.Add(invitation);

                await SendEmailTemplate(userContact);

                Notification notif = await _notificationService.Create(User.NameIdentifier(), "Demande de contact", "contact/list", "reminder", "reminder", "reminder", "add-contact");
                return Ok(new ContactViewModel() { ViewContact = new ViewContact(user, userContact), Notification = notif });
            }
            return Conflict();
        }
        private async Task SendEmailTemplate(ApplicationUser user)
        {
            string data1 = CryptHelper.Rijndael.Encrypt($"{user.Id}", _configuration["QrCode:Key"]);
            string data2 = CryptHelper.Rijndael.Encrypt($"{User.NameIdentifier()}", _configuration["QrCode:Key"]);

            var callbackUrl = $"{Request.Scheme}://{Request.Host}/Identity/Account/ConfirmContact?data1={data1}&data2={data2}";

            var frontWebSite = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var logoUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/assets/img/logo_full.png");
            var unSubscribeUrl = await _urlShortner.CreateUrl(Request.Scheme, Request.Host, UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/Account/UnSubscribe"));
            var login = $"{user.DisplayName}";

            callbackUrl = await _urlShortner.CreateUrl(Request.Scheme, Request.Host, callbackUrl);

            var parameters = new Dictionary<string, string>()
                        {
                            {"login", login },
                            {"frontWebSite", frontWebSite },
                            {"logoUrl", logoUrl },
                            {"unSubscribeUrl", unSubscribeUrl },
                            {"callbackUrl", callbackUrl }
                        };

            var message = GenerateHtmlTemplate(_serviceProvider, _env.WebRootPath, "contact-confirmation", parameters);
            await _emailSender.SendEmailAsync(user.Email, "Add Contact", message);
        }

        [HttpGet("Suggestions")]
        public async Task<ActionResult<IEnumerable<ViewContact>>> GetSuggestion(string term, string filter)
        {
            if (string.IsNullOrEmpty(term))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(User.NameIdentifier());

            var suggestions = await Task.Run(() => _userManager?.Users.ToList().Where(c => c.EmailConfirmed && (filter == null || c.Country == filter)
                          && (c.FirstName != null && c.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase)
                          || c.LastName != null && c.LastName.Contains(term, StringComparison.OrdinalIgnoreCase)
                          || c.Description != null && c.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                         c.DisplayName != null && c.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                         c.Email != null && c.Email.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                         c.Title != null && c.Title.Contains(term, StringComparison.OrdinalIgnoreCase))));

            if (suggestions == null)
                return NotFound();

            List<ViewContact> contacts = new List<ViewContact>();
            var allUserContacts = await _UnitOfWork.ContactRepository.Where(x => x.UserId == User.NameIdentifier() || x.OwnerId == User.NameIdentifier());
            suggestions = suggestions.Where(u => !allUserContacts.Any(c => u.Id == c.OwnerId || u.Id == c.UserId || u.Id == User.NameIdentifier())).ToList();

            foreach (var suggestion in suggestions)
            {
                contacts.Add(new ViewContact(user, suggestion));
            }

            return Ok(contacts);
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
