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
using Abeer.Data.UnitOfworks;
using System;
using Abeer.Services;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Hosting;
using Abeer.Shared.ReferentielTable;
using Abeer.Shared.Technical;
using Microsoft.Extensions.Configuration;
using Abeer.Shared.Functional;
using System.Globalization;
using static Abeer.Services.TemplateRenderManager;

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
        private readonly IEmailSenderService _emailSender;
        private readonly IConfiguration _configuration;
        private readonly NotificationService _notificationService;
        EventTrackingService _eventTrackingService;

        public ContactsController(NotificationService notificationService,
            IWebHostEnvironment env, IConfiguration configuration,
            IServiceProvider serviceProvider,
            IEmailSenderService emailSender, FunctionalUnitOfWork onlineWalletUnitOfWork, UserManager<ApplicationUser> userManager,
            EventTrackingService eventTrackingService)
        {
            _notificationService = notificationService;
            _serviceProvider = serviceProvider;
            _UnitOfWork = onlineWalletUnitOfWork;
            _userManager = userManager;
            _eventTrackingService = eventTrackingService;
            _emailSender = emailSender;
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
                foreach(var contact in contacts)
                {
                    var uContact = await _userManager.FindByIdAsync(contact.UserId);
                    ViewContact item = new(user, uContact, contact);

                    user.NubmerOfView += 1;
                    await _userManager.UpdateAsync(user);

                    item.Contact.NumberOfView = user.NubmerOfView;
                    item.Contact.SocialNetworkConnected = await _UnitOfWork.SocialNetworkRepository.GetSocialNetworkLinks(item.UserId) ??
                        new List<SocialNetwork>();

                    item.Contact.CustomLinks = await _UnitOfWork.CustomLinkRepository.GetCustomLinkLinks(item.UserId) ?? new List<CustomLink>();

                    item.Contact.NumberOfContacts = (await _UnitOfWork.ContactRepository.GetContacts(item.Contact.Id)).Count;
                    item.Contact.NumberOfAds = (await _UnitOfWork.AdRepository.GetVisibledUser(item.Contact.Id)).Count;
                    
                    viewContacts.Add(item);
                }
            }

            return Ok(viewContacts);
        }

        [HttpGet("requests")]
        public async Task<ActionResult<IEnumerable<ViewContact>>> GetContactRequests()
        {
            List<ViewContact> viewContacts = new List<ViewContact>();

            var user = await _userManager.FindByIdAsync(User.NameIdentifier());

            var contacts = (await _UnitOfWork.ContactRepository.GetContactRequests(user.Id))?.ToList();

            if (contacts.Any())
            {
                foreach (var contact in contacts)
                {
                    var uContact = await _userManager.FindByIdAsync(contact.OwnerId);
                    ViewContact item = new(user, uContact, contact);

                    user.NubmerOfView += 1;
                    await _userManager.UpdateAsync(user);

                    item.Contact.NumberOfView = user.NubmerOfView;
                    item.Contact.SocialNetworkConnected = await _UnitOfWork.SocialNetworkRepository.GetSocialNetworkLinks(item.UserId) ??
                        new List<SocialNetwork>();

                    item.Contact.CustomLinks = await _UnitOfWork.CustomLinkRepository.GetCustomLinkLinks(item.UserId) ?? new List<CustomLink>();

                    item.Contact.NumberOfContacts = (await _UnitOfWork.ContactRepository.GetContacts(item.Contact.Id)).Count;
                    item.Contact.NumberOfAds = (await _UnitOfWork.AdRepository.GetVisibledUser(item.Contact.Id)).Count;

                    viewContacts.Add(item);
                }
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

                Notification notif = await _notificationService.Create(contactId, "Demande de contact", "contact/list", "reminder", "reminder", "reminder", "add-contact");
                return Ok(new ContactViewModel() { ViewContact = new ViewContact(user, userContact), Notification = notif });
            }
            return Conflict();
        }

        [HttpDelete("{contactLinkId}")]
        public async Task<ActionResult<ViewContact>> Remove(Guid contactLinkId)
        {
            var user = await _userManager.FindByIdAsync(User.NameIdentifier());

            var contact = await _UnitOfWork.ContactRepository.GetContact(contactLinkId);

            if (contact == null)
                return NotFound();

            _UnitOfWork.ContactRepository.Remove(contact);

            await _notificationService.Create(contact.UserId, "Suppression de contact", "contact/list", "reminder", "reminder", "reminder", "remove-contact");
            return Ok();
        }

        [HttpPut("accept/{contactId}")]
        public async Task<ActionResult<Contact>> AcceptInvitation(Guid contactId, Contact link)
        {
            if (contactId == Guid.Empty)
                return BadRequest();

            if (contactId != link.Id)
                return BadRequest();

            var contact = await _UnitOfWork.ContactRepository.GetContact(contactId);

            if (contact == null)
                return NotFound();

            contact.DateAccepted = DateTime.Now;
            contact.UserAccepted = EnumUserAccepted.ACCEPTED;

            await _UnitOfWork.ContactRepository.Update(contact);

            var invitation = await _UnitOfWork.InvitationRepository.GetInvitation(contact.OwnerId, contact.UserId);

            await _eventTrackingService.Create(new Shared.Functional.EventTrackingItem
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                Category = "contact",
                Key = "confirmContact",
                UserId = contact.OwnerId
            });

            if (invitation != null)
            {
                invitation.InvitationStat = (int)EnumUserAccepted.ACCEPTED;
                await _UnitOfWork.InvitationRepository.Update(invitation);
            }

            var contact2 = await _UnitOfWork.ContactRepository.GetContact(contact.UserId, contact.OwnerId);

            if (contact2 == null)
            {
                contact2 = new Contact
                {
                    OwnerId = contact.UserId,
                    UserId = contact.OwnerId,
                    DateAccepted = DateTime.Now,
                    UserAccepted = EnumUserAccepted.ACCEPTED
                };

                await _UnitOfWork.ContactRepository.Add(contact2);

                var invitation2 = await _UnitOfWork.InvitationRepository.GetInvitation(contact.UserId, contact.OwnerId);

                if (invitation2 != null)
                {
                    invitation.InvitationStat = (int)EnumUserAccepted.ACCEPTED;
                    await _UnitOfWork.InvitationRepository.Update(invitation);
                }
                else
                {
                    invitation2 = new Shared.Functional.Invitation
                    {
                        OwnedId = contact.UserId,
                        ContactId = contact.OwnerId,
                        CreatedDate = DateTime.UtcNow,
                        InvitationStat = (int)EnumUserAccepted.ACCEPTED,
                        Id = Guid.NewGuid()
                    };

                    await _UnitOfWork.InvitationRepository.Add(invitation2);

                    await _eventTrackingService.Create(new Shared.Functional.EventTrackingItem
                    {
                        Id = Guid.NewGuid(),
                        CreatedDate = DateTime.UtcNow,
                        Category = "contact",
                        Key = "confirmContact",
                        UserId = contact2.OwnerId
                    });
                }
            }

            return Ok(contact);
        }

        private async Task SendEmailTemplate(ApplicationUser user)
        {
            string data1 = CryptHelper.Rijndael.Encrypt($"{user.Id}", _configuration["QrCode:Key"]);
            string data2 = CryptHelper.Rijndael.Encrypt($"{User.NameIdentifier()}", _configuration["QrCode:Key"]);

            var callbackUrl = $"{Request.Scheme}://{Request.Host}/Identity/Account/ConfirmContact?data1={data1}&data2={data2}";

            var frontWebSite = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var logoUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/assets/img/logo_full.png");
            var unSubscribeUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/Account/UnSubscribe");
            var login = $"{user.DisplayName}";

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

            List<ViewContact> contacts = new();
            var allUserContacts = await _UnitOfWork.ContactRepository.Where(x => x.UserId == User.NameIdentifier() || x.OwnerId == User.NameIdentifier());
            suggestions = suggestions.Where(u => !allUserContacts.Any(c => u.Id == c.OwnerId || u.Id == c.UserId || u.Id == User.NameIdentifier())).ToList();

            foreach (var suggestion in suggestions)
            {
                if (suggestion.Id == user.Id)
                    continue;

                var item = new ViewContact(user, suggestion);
                item.Contact.NumberOfContacts = (await _UnitOfWork.ContactRepository.GetContacts(item.Contact.Id)).Count;
                item.Contact.NumberOfAds = (await _UnitOfWork.AdRepository.GetVisibledUser(item.Contact.Id)).Count;
                contacts.Add(item);
            }

            return Ok(contacts);
        }

        [HttpGet("find")]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> Find(string term)
        {
            if (string.IsNullOrEmpty(term))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(User.NameIdentifier());
            var contacts = await _UnitOfWork.ContactRepository.GetContacts(user.Id, EnumUserAccepted.NO_REQUEST);

            var suggestions = await Task.Run(() => _userManager?.Users.ToList().Where(c => 
                (c.FirstName != null && c.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || c.LastName != null && c.LastName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || c.Description != null && c.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                c.DisplayName != null && c.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                c.Email != null && c.Email.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                c.Title != null && c.Title.Contains(term, StringComparison.OrdinalIgnoreCase))));

            if (suggestions == null || contacts.Any() == false)
                return NotFound();

            suggestions = suggestions.Where(s => contacts.Any(c => c.UserId == s.Id)).ToList();

            return Ok(suggestions);
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

        // GET: api/Contacts/5
        [HttpGet("getbycontactid/{contactId}")]
        public async Task<ActionResult<Contact>> GetContactByContactId(string contactId)
        {
            if (string.IsNullOrEmpty(contactId))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(User.NameIdentifier());
            var ucontact = await _userManager.FindByIdAsync(contactId);
            
            if (user == null || ucontact == null)
                return BadRequest();

            var contact = await _UnitOfWork.ContactRepository.GetContact(ucontact.Id, user.Id);

            if (contact == null)
            {
                return NotFound();
            }

            return Ok(contact);
        }

        [HttpGet("link/{contactId}")]
        public async Task<ActionResult<Contact>> GetLinkForProfile(string contactId)
        {
            if (string.IsNullOrEmpty(contactId))
                return BadRequest();

            var userId = User.NameIdentifier();

            var contact = await _UnitOfWork.ContactRepository.FirstOrDefault(p=>(p.OwnerId == contactId && p.UserId == userId) || (p.OwnerId == userId && p.UserId == contactId));

            if (contact == null)
            {
                return NotFound();
            }

            return Ok(contact);
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

        [AllowAnonymous]
        [HttpGet("countries")]
        public async Task<ActionResult<IList<Country>>> GetCountries()
        {
            var contacts = await _userManager.Users.ToListAsync();
            var countries = await _UnitOfWork.CountriesRepository.GetCountries(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            countries = countries.Where(c => contacts.Any(co => co.Country.Equals(c.Eeacode, StringComparison.OrdinalIgnoreCase))).ToList();
            return Ok(countries);
        }

        [AllowAnonymous]
        [HttpGet("bycountry/{countryEacode}")]
        public async Task<ActionResult<IList<ViewContact>>> GetContactsByCountry(string countryEacode)
        {

            var suggestions = await _userManager?.Users.Where(c => c.Country.Equals(countryEacode)).ToListAsync();

            if (suggestions == null)
                return NotFound();

            var contacts = new List<ViewContact>();

            ApplicationUser user = null;
            IList<Contact> links = new List<Contact>();

            if (User.Identity.IsAuthenticated)
            {
                user = await _userManager.FindByIdAsync(User.NameIdentifier());
                links = await _UnitOfWork.ContactRepository.GetContacts(user.Id);
            }

            foreach (var suggestion in suggestions)
            {
                Contact link = null;

                if(user != null && links != null)
                {
                    link = links.FirstOrDefault(l => l.UserId == suggestion.Id && l.OwnerId == user.Id);
                }

                var item = new ViewContact(user, suggestion, link);
                item.Contact.NumberOfContacts = (await _UnitOfWork.ContactRepository.GetContacts(item.Contact.Id)).Count;
                item.Contact.SocialNetworkConnected = await _UnitOfWork.SocialNetworkRepository.GetSocialNetworkLinks(item.Contact.Id);
                item.Contact.CustomLinks = await _UnitOfWork.CustomLinkRepository.GetCustomLinkLinks(item.Contact.Id);
                item.Contact.NumberOfAds = (await _UnitOfWork.AdRepository.GetVisibledUser(item.Contact.Id)).Count;

                contacts.Add(item);
            }

            return Ok(contacts);
        }

        [HttpGet("profileorganization/{contactiD}")]
        public async Task<ActionResult<ProfileOrganizationViewModel>> GetProfileOrganization(string contactId)
        {
            var user = (ViewApplicationUser)await _userManager.FindByIdAsync(User.NameIdentifier());
            
            if (user == null)
                return BadRequest();

            if (!user.IsUltimate && !user.IsAdmin && !user.IsUnlimited)
                return BadRequest();

            var profileOrganization = await _UnitOfWork.ContactRepository.GetOrganization(contactId);
    
            if(profileOrganization != null && !string.IsNullOrEmpty(profileOrganization.ManagerId))
                profileOrganization.Manager = await _userManager.FindByIdAsync(profileOrganization.ManagerId);

            return Ok(profileOrganization);
        }

        [HttpGet("organization")]
        public async Task<ActionResult<IList<Organization>>> SearchOrganization([FromQuery]string searchTerm)
        {
            var user = (ViewApplicationUser)await _userManager.FindByIdAsync(User.NameIdentifier());

            if (user == null)
                return BadRequest();

            if (!user.IsUltimate && !user.IsAdmin && !user.IsUnlimited)
                return BadRequest();

            var organizations = await _UnitOfWork.OrganizationRepository.Where(o=>o.Name.Contains(searchTerm) || o.Description.Contains(searchTerm));

            return Ok(organizations);
        }

        [HttpPost("organization")]
        public async Task<ActionResult<Organization>> CreateOrganization(Organization organization)
        {
            var user = (ViewApplicationUser)await _userManager.FindByIdAsync(User.NameIdentifier());

            if (user == null)
                return BadRequest();

            if (!user.IsUltimate && !user.IsAdmin && !user.IsUnlimited)
                return BadRequest();

            var found = await _UnitOfWork.OrganizationRepository.FirstOrDefault(o => o.Name.Contains(organization.Name) || o.Description.Contains(organization.Name));
            
            if (found != null)
                return BadRequest();

            await _UnitOfWork.OrganizationRepository.Add(organization);
            return Ok(organization);
        }

        [HttpGet("team/{organizationId}")]
        public async Task<ActionResult<IList<Team>>> SearchTeam(Guid organizationId, [FromQuery] string searchTerm)
        {
            var user = (ViewApplicationUser)await _userManager.FindByIdAsync(User.NameIdentifier());

            if (user == null)
                return BadRequest();

            if (!user.IsUltimate && !user.IsAdmin && !user.IsUnlimited)
                return BadRequest();

            var teams = await _UnitOfWork.TeamRepository.Where(o => o.OrganizationId == organizationId && (o.Name.Contains(searchTerm) || o.Description.Contains(searchTerm)));

            return Ok(teams);
        }

        [HttpPost("team/{organizationId}")]
        public async Task<ActionResult<Team>> CreateTeam(Guid organizationId, Team team)
        {
            var user = (ViewApplicationUser)await _userManager.FindByIdAsync(User.NameIdentifier());

            if (user == null)
                return BadRequest();

            if (!user.IsUltimate && !user.IsAdmin && !user.IsUnlimited)
                return BadRequest();

            if (organizationId == Guid.Empty)
                return BadRequest();

            if (team.OrganizationId != organizationId)
                return BadRequest();

            var organization = await _UnitOfWork.OrganizationRepository.GetOrganization(organizationId);

            if (organization == null)
                return NotFound();

            var found = await _UnitOfWork.TeamRepository.FirstOrDefault(o => o.OrganizationId == organizationId && (o.Name.Contains(team.Name) || o.Description.Contains(team.Name)));

            if (found != null)
                return BadRequest();

            await _UnitOfWork.TeamRepository.Add(team);
            return Ok(team);
        }
        [HttpGet("manager/{organizationId}/{teamId}")]
        public async Task<ActionResult<IList<ViewApplicationUser>>> SearchManager(Guid organizationId, Guid teamId, [FromQuery]string searchTerm)
        {
            var user = (ViewApplicationUser)await _userManager.FindByIdAsync(User.NameIdentifier());

            if (user == null)
                return BadRequest();

            if (!user.IsUltimate && !user.IsAdmin && !user.IsUnlimited)
                return BadRequest();


            if (organizationId == Guid.Empty)
                return BadRequest();

            if (teamId == Guid.Empty)
                return BadRequest();

            var organization = await _UnitOfWork.OrganizationRepository.GetOrganization(organizationId);

            if (organization == null)
                return NotFound();

            var team = await _UnitOfWork.TeamRepository.GetTeam(teamId);

            if (team == null)
                return NotFound();

            var profiles = await _UnitOfWork.ContactRepository.GetProfilesByTeam(organizationId, teamId);

            if (profiles.Any())
            {
                var ids = profiles.Select(p => p.ContactId).ToArray();

                var users = await _userManager.Users.Where(u => (u.FirstName.Contains(searchTerm) || u.LastName.Contains(searchTerm) || u.Description.Contains(searchTerm) ||
                    u.DisplayName.Contains(searchTerm) || u.Email.Contains(searchTerm) || u.Title.Contains(searchTerm)) && ids.Contains(u.Id)).ToListAsync();

                return Ok(users);
            }
            else
                return Ok();
        }

        [HttpPut("profileOrganization/{contactId}")]
        public async Task<IActionResult> SetProfileOrganization(string contactId, ProfileOrganizationViewModel profileOrganizationViewModel)
        {
            if (contactId == Guid.Empty.ToString())
                return BadRequest();

            if (contactId != profileOrganizationViewModel.ContactId)
                return BadRequest();

            var profile = await _UnitOfWork.ContactRepository.GetOrganization(contactId);
            
            if(profile == null)
            {
                await _UnitOfWork.ContactRepository.AddOrganization(profileOrganizationViewModel);
            }
            else 
            {
                await _UnitOfWork.ContactRepository.UpdateOrganization(profileOrganizationViewModel);
            }

            return Ok(profileOrganizationViewModel);
        }

        [HttpGet("byorganization")]
        public async Task<ActionResult<IList<ViewContact>>> GetContactsByOrganization()
        {
            var profileOrg = await _UnitOfWork.ContactRepository.GetOrganization(User.NameIdentifier());
            
            if (profileOrg == null)
                return BadRequest();

            var profiles = await _UnitOfWork.ContactRepository.GetProfilesByOrganization(profileOrg.OrganizationId);
            var userIds = profiles.Select(p => p.ContactId);

            ApplicationUser user = null;
            IList<Contact> links = new List<Contact>();

            if (User.Identity.IsAuthenticated)
            {
                user = await _userManager.FindByIdAsync(User.NameIdentifier());
                links = await _UnitOfWork.ContactRepository.GetContacts(user.Id);
            }

            var suggestions = await _userManager?.Users.Where(c => userIds.Contains(c.Id) && c.Id != user.Id).ToListAsync();

            if (suggestions == null)
                return NotFound();

            var contacts = new List<ViewContact>();


            foreach (var suggestion in suggestions)
            {
                Contact link = null;

                if (user != null && links != null)
                {
                    link = links.FirstOrDefault(l => l.UserId == suggestion.Id && l.OwnerId == user.Id);
                }

                var item = new ViewContact(user, suggestion, link);
                item.Contact.NumberOfContacts = (await _UnitOfWork.ContactRepository.GetContacts(item.Contact.Id)).Count;
                item.Contact.SocialNetworkConnected = await _UnitOfWork.SocialNetworkRepository.GetSocialNetworkLinks(item.Contact.Id);
                item.Contact.CustomLinks = await _UnitOfWork.CustomLinkRepository.GetCustomLinkLinks(item.Contact.Id);
                item.Contact.NumberOfAds = (await _UnitOfWork.AdRepository.GetVisibledUser(item.Contact.Id)).Count;

                contacts.Add(item);
            }

            return Ok(contacts);
        }

        [HttpGet("byteam")]
        public async Task<ActionResult<IList<ViewContact>>> GetContactsByTeam()
        {
            var profileOrg = await _UnitOfWork.ContactRepository.GetOrganization(User.NameIdentifier());

            if (profileOrg == null)
                return BadRequest();

            var profiles = await _UnitOfWork.ContactRepository.GetProfilesByTeam(profileOrg.OrganizationId, profileOrg.TeamId);
            var userIds = profiles.Select(p => p.ContactId);

            ApplicationUser user = null;
            IList<Contact> links = new List<Contact>();

            if (User.Identity.IsAuthenticated)
            {
                user = await _userManager.FindByIdAsync(User.NameIdentifier());
                links = await _UnitOfWork.ContactRepository.GetContacts(user.Id);
            }

            var suggestions = await _userManager?.Users.Where(c => userIds.Contains(c.Id) && c.Id != user.Id).ToListAsync();

            if (suggestions == null)
                return NotFound();

            var contacts = new List<ViewContact>();

            foreach (var suggestion in suggestions)
            {
                Contact link = null;

                if (user != null && links != null)
                {
                    link = links.FirstOrDefault(l => l.UserId == suggestion.Id && l.OwnerId == user.Id);
                }

                var item = new ViewContact(user, suggestion, link);
                item.Contact.NumberOfContacts = (await _UnitOfWork.ContactRepository.GetContacts(item.Contact.Id)).Count;
                item.Contact.SocialNetworkConnected = await _UnitOfWork.SocialNetworkRepository.GetSocialNetworkLinks(item.Contact.Id);
                item.Contact.CustomLinks = await _UnitOfWork.CustomLinkRepository.GetCustomLinkLinks(item.Contact.Id);
                item.Contact.NumberOfAds = (await _UnitOfWork.AdRepository.GetVisibledUser(item.Contact.Id)).Count;

                contacts.Add(item);
            }

            return Ok(contacts);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendProfile(SendProfileViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var message = new Message
            {
                DateSent = DateTime.UtcNow,
                Text = viewModel.Subject + Environment.NewLine + viewModel.Body + Environment.NewLine + viewModel.TargetUrl,
                UserIdFrom = Guid.Parse(User.NameIdentifier()),
                UserIdTo = Guid.Parse(viewModel.SendToId)
            };

            var user = await _userManager.FindByIdAsync(User.NameIdentifier());
            var sendTo = await _userManager.FindByIdAsync(viewModel.SendToId);

            await _UnitOfWork.MessageRepository.Add(message);
            await SendProfileEmail(message, user, sendTo);
            await _notificationService.Create(sendTo.Id, $"{sendTo.DisplayName} a partagé avec vous le profil de {user.DisplayName}", viewModel.TargetUrl, 
                "sendProfile", "sendProfile", "sendProfile", "sendProfile");

            return Ok();
        }

        private async Task SendProfileEmail(Message message, ApplicationUser user, ApplicationUser sendTo)
        {
            var code = "";

            if (!string.IsNullOrEmpty(sendTo.PinDigit))
                code = sendTo.PinDigit;
            else
                code = sendTo.Id;

            var callbackUrl = $"{Request.Scheme}://{Request.Host}/viewProfile/{code}";

            var frontWebSite = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var logoUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/assets/img/logo_full.png");
            var unSubscribeUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/Account/UnSubscribe");
            var login = $"{user.DisplayName}";

            var parameters = new Dictionary<string, string>()
                        {
                            {"login", login },
                            {"frontWebSite", frontWebSite },
                            {"logoUrl", logoUrl },
                            {"unSubscribeUrl", unSubscribeUrl },
                            {"callbackUrl", callbackUrl },
                            {"sendTo", sendTo.DisplayName },
                            {"sendFrom", user.DisplayName }
                        };

            var html = GenerateHtmlTemplate(_serviceProvider, _env.WebRootPath, "sendprofile", parameters);
            await _emailSender.SendEmailAsync(sendTo.Email, $"{user.DisplayName} partage avec vous un profil meetag {sendTo.DisplayName}", html);
        }
    }
}