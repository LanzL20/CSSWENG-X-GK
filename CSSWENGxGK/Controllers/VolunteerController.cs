using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CSSWENGxGK.Controllers
{
    public class VolunteerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<Volunteer> _userManager;

        public VolunteerController(ApplicationDbContext db, UserManager<Volunteer> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Volunteer model)
        {
            if (ModelState.IsValid)
            {
                var volunteer = new Volunteer
                {
                    CreatedDate = DateTime.Now,
                    LastUpdateDate = DateTime.Now,
                    IsDeleted = false,
                    IsActive = false,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    MobileNumber = model.MobileNumber,
                    // Set other properties here
                };

                // Add the Volunteer to the database
                _db.T_Volunteer.Add(volunteer);
                _db.SaveChanges();

                // Assign the "Volunteer" role
                await _userManager.AddToRoleAsync(volunteer, "Volunteer");

                return RedirectToAction("Register");
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Login(Volunteer model)
        {
            if (ModelState.IsValid)
            {
                var user = _db.T_Volunteer
                    .FirstOrDefault(v => v.Email == model.Email && v.Password == model.Password);

                if (user != null)
                {
                    return RedirectToAction("Dashboard", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }
            return View(model);
        }
    }
}
