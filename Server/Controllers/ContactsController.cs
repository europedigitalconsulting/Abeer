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
using Abeer.Shared.ViewModels;

namespace Abeer.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FunctionalUnitOfWork _UnitOfWork;

        public ContactsController(FunctionalUnitOfWork onlineWalletUnitOfWork, UserManager<ApplicationUser> userManager)
        {
            _UnitOfWork = onlineWalletUnitOfWork;
            _userManager = userManager;
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
        
        [HttpGet("import/{id}")]
        public async Task<ActionResult<ImportContactResultViewModel>> Import(string id)
        {
            if (User.NameIdentifier() == id)
            {
                return new ImportContactResultViewModel { Contact = null, IsValid = false, StatusCode = "SelfReference" };
            }
            else
            {
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

                    return new ImportContactResultViewModel { Contact = result, IsValid = true, StatusCode = "OK" };
                }
                else
                {
                    return new ImportContactResultViewModel { Contact = contact, IsValid = false, StatusCode = "Duplicate" };
                }
            }
        }

        [HttpGet("Suggestions")]
        public async Task<ActionResult<IEnumerable<ViewContact>>> GetSuggestion(string term)
        {
            if (string.IsNullOrEmpty(term))
                return BadRequest();

            var users = await Task.Run(()=>_userManager?.Users.ToList().Where(c=>
                 c.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase)
                        || c.LastName.Contains(term, StringComparison.OrdinalIgnoreCase)
                        || c.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        c.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        c.Email.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        c.Title.Contains(term, StringComparison.OrdinalIgnoreCase)));

            if (users == null)
                return NotFound();

            ConcurrentBag<ViewContact> contacts = new ConcurrentBag<ViewContact>();
            var allUserContacts = await _UnitOfWork.ContactRepository.GetContacts(User.NameIdentifier());
            users = users.Where(u => !allUserContacts.Any(c => u.Id == c.UserId || u.Id == User.NameIdentifier()));

            if (users != null)
            {
                users.ToList().ForEach(async (user) =>
                {
                    var contact = await _UnitOfWork.ContactRepository.FirstOrDefault(u => u.UserId == user.Id);
                    if (contact != null)
                    {
                        var result = users.FirstOrDefault(c => c.Id == contact.UserId);
                        ViewContact item = new ViewContact(result, contact);
                        contacts.Add(item);
                    }

                });
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
