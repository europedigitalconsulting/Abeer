using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Abeer.Shared;
using Abeer.Shared.Technical;
using Microsoft.Extensions.Configuration;
using Abeer.Data.UnitOfworks;
using System;
using Abeer.Shared.ReferentielTable;

namespace Abeer.Server.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmContactModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly FunctionalUnitOfWork _UnitOfWork;

        public ConfirmContactModel(FunctionalUnitOfWork functionalUnitOfWork, IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _UnitOfWork = functionalUnitOfWork;
        }
         
        public string DisplayName { get; set; }
        public async Task<IActionResult> OnGetAsync(string data1, string data2)
        {
            if (string.IsNullOrEmpty(data1)|| string.IsNullOrEmpty(data2))
            {
                return RedirectToPage("/Index");
            } 
            var ownerId = CryptHelper.Rijndael.Decrypt(data1, _configuration["QrCode:Key"]);
            var userId =  CryptHelper.Rijndael.Decrypt(data2, _configuration["QrCode:Key"]);

            var owner = await _userManager.FindByIdAsync(ownerId);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || owner == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var contact = await _UnitOfWork.ContactRepository.GetContact(ownerId, userId);
            contact.DateAccepted = DateTime.Now;
            contact.UserAccepted = EnumUserAccepted.ACCEPTED;
            await _UnitOfWork.ContactRepository.Update(contact);

            var contact2 = await _UnitOfWork.ContactRepository.GetContact(userId, ownerId);
            if (contact2 == null)
            {
                contact2 = new Contact();
                contact2.OwnerId = contact.UserId;
                contact2.UserId = contact.OwnerId;
                contact2.DateAccepted = DateTime.Now;
                contact2.UserAccepted = EnumUserAccepted.ACCEPTED;
                await _UnitOfWork.ContactRepository.Add(contact2);
            }

            DisplayName = user.DisplayName;

            return Page();
        }
        private bool CheckValidation(ApplicationUser user)
        {
            if (user != null && user.EmailConfirmed)
            {
                return true;
            }
            return false;
        }
        public async Task<IActionResult> OnPostAsync()
        {
            return await Task.Run(() =>
            {
                var returnUrl = Url.Content("~/");
                return LocalRedirect(returnUrl);
            });
        }
    }
}
