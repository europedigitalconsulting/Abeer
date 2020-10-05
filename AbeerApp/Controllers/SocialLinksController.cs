using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AbeerContactShared.Data.Repo;
using WebApplication1.Data;
using WebApplication1.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace AbeerContactShared.Controllers
{
    public class SocialLinksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;

        public SocialLinksController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: SocialLinks
        public async Task<IActionResult> Index()
        {
            return View(await _context.SocialLinks.ToListAsync());
        }

        // GET: SocialLinks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var socialLink = await _context.SocialLinks
                .FirstOrDefaultAsync(m => m.ID == id);
            if (socialLink == null)
            {
                return NotFound();
            }

            return View(socialLink);
        }

        // GET: SocialLinks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SocialLinks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,IdUser,UrlSocialProfile,TypeSocialLink")] SocialLink socialLink)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);
                socialLink.IdUser = user.Id;
                _context.Add(socialLink);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(socialLink);
        }

        // GET: SocialLinks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var socialLink = await _context.SocialLinks.FindAsync(id);
            if (socialLink == null)
            {
                return NotFound();
            }
            return View(socialLink);
        }

        // POST: SocialLinks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,IdUser,UrlSocialProfile,TypeSocialLink")] SocialLink socialLink)
        {
            if (id != socialLink.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ApplicationUser user = await _userManager.GetUserAsync(User);
                    socialLink.IdUser = user.Id;
                    _context.Update(socialLink);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SocialLinkExists(socialLink.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(socialLink);
        }

        // GET: SocialLinks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var socialLink = await _context.SocialLinks
                .FirstOrDefaultAsync(m => m.ID == id);
            if (socialLink == null)
            {
                return NotFound();
            }

            return View(socialLink);
        }

        // POST: SocialLinks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var socialLink = await _context.SocialLinks.FindAsync(id);
            _context.SocialLinks.Remove(socialLink);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SocialLinkExists(int id)
        {
            return _context.SocialLinks.Any(e => e.ID == id);
        }
    }
}
