using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Abeer.Server.Data;
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
            var contacts = await _UnitOfWork.ContactRepository.GetContacts(User.NameIdentifier());
            
            List<ViewContact> viewContacts = new List<ViewContact>();

            Parallel.ForEach(contacts, async (contact) =>
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

                var contact = await _UnitOfWork.ContactRepository.FirstOrDefaultAsync(c => c.UserId == user.Id && c.OwnerId == User.NameIdentifier());

                if (contact == null)
                {
                    var result = await _UnitOfWork.ContactRepository.AddAsync(new Contact
                    {
                        OwnerId = User.NameIdentifier(), 
                        UserId = user.Id
                    });

                    await _UnitOfWork.SaveChangesAsync();

                    return new ImportContactResultViewModel { Contact = result, IsValid = true, StatusCode = "OK" };
                }
                else
                {
                    return new ImportContactResultViewModel { Contact = contact, IsValid = false, StatusCode = "Duplicate" };
                }
            }
        }

        [HttpGet("Suggestion")]
        public  async Task<ActionResult<IEnumerable<ViewContact>>> GetSuggestion(string term)
        {
            var users = await _userManager?.Users.Where(u => u.Id.Contains(term) ||
                u.Email.Contains(term) || u.UserName.Contains(term)).ToListAsync();

            ConcurrentBag<ViewContact> contacts = new ConcurrentBag<ViewContact>();

            Parallel.ForEach<ApplicationUser>(users, async (user) =>
            {
                var contact = await _UnitOfWork.ContactRepository.FirstOrDefaultAsync(u => u.UserId == user.Id);

                if (contact != null)
                {
                    contacts.Add(new ViewContact { UserId = user.Id, DisplayName = user.DisplayName, Email = user.Email });
                }
                else
                {
                    contacts.Add(new ViewContact { UserId = user.Id, DisplayName = user.UserName, Email = user.Email });
                }
            });

            return contacts;
        }

        // GET: api/Contacts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Contact>> GetContact(long id)
        {
            var contact = await _UnitOfWork.ContactRepository.GetContact(id);

            if (contact == null)
            {
                return NotFound();
            }

            return contact;
        }

        // PUT: api/Contacts/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContact(long id, Contact contact)
        {
            if (id != contact.Id)
            {
                return BadRequest();
            }

            await _UnitOfWork.ContactRepository.Update(contact);
            await _UnitOfWork.SaveChangesAsync();

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

            await _UnitOfWork.ContactRepository.AddAsync(contact);
            await _UnitOfWork.SaveChangesAsync();

            return CreatedAtAction("GetContact", new { id = contact.Id }, contact);
        }

        // DELETE: api/Contacts/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Contact>> DeleteContact(long id)
        {
            var contact = await _UnitOfWork.ContactRepository.FindAsync(id);
            
            if (contact == null)
            {
                return NotFound();
            }

            _UnitOfWork.ContactRepository.Remove(contact);
            await _UnitOfWork.SaveChangesAsync();

            return contact;
        }
    }
}
