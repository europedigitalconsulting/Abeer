using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Areas.Identity.Data;
using WebApplication1.Data;

namespace AbeerContactShared.Controllers
{
    public class MyAccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;

        public MyAccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: MyAccountController
        public async Task<ActionResult> IndexAsync()
        {

            ApplicationUser user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        // GET: MyAccountController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: MyAccountController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MyAccountController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(IndexAsync));
            }
            catch
            {
                return View();
            }
        }

        // GET: MyAccountController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: MyAccountController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(IndexAsync));
            }
            catch
            {
                return View();
            }
        }

        // GET: MyAccountController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: MyAccountController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(IndexAsync));
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public bool UpdateProfil(string firstName, string idUserProfil, string pathImage, string lastName, string username, string email)
        {
            try
            {
                if (_context != null)
                {            
                    var user = _context.Utilisateurs.Where(x => x.Id == idUserProfil).FirstOrDefault();
                    if (user != null)
                    {
                        user.FirstName = firstName;
                        user.Email = email;
                        user.LastLogin = DateTime.Now;
                        user.LastName = lastName;
                        user.PathImage = pathImage;
                        user.UserName = username;
                        _context.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                var x = ex.Message;
                return false;
            }
            return true;
        }
    }
}
