using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CSSWENGxGK.Controllers
{
    public class VolunteerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public VolunteerController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
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
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create a Volunteer object based on the RegisterViewModel
                var volunteer = new Volunteer
                {
                    CreatedDate = DateTime.Now,
                    LastUpdateDate = DateTime.Now,
                    IsDeleted = false,
                    IsActive = false,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.MobileNumber,
                    // Set other properties here
                };

                // Add the Volunteer to the database and save changes
                _db.T_Volunteer.Add(volunteer);
                _db.SaveChanges();

                // Assign the "User" role to the new user
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }
                else
                {
                    // Handle user creation errors
                    // ...
                }

                return RedirectToAction("Register");
            }

            // If ModelState is not valid, return the registration view with validation errors
            return View(model);
        }
    }
}
