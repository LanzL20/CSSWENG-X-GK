using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Data.SqlClient;

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
        public IActionResult VerifyVolunteerID(string Vol_ID)
        {
            Console.WriteLine(Vol_ID);
            if (int.TryParse(Vol_ID, out int parsedVolunteerID))
            {
                if (parsedVolunteerID <= 0)
                {
                    return Json(new { found = false });
                }

                // Assuming _db is your DbContext, query the database to find the volunteer with the given ID
                var volunteer = _db.T_Volunteer.FirstOrDefault(v => v.VolunteerID == parsedVolunteerID);

                // Check if the volunteer was found in the database
                if (volunteer != null)
                {
                    // The volunteerID exists in the database
                    return Json(new
                    {
                        found = true,
                        volunteerID = parsedVolunteerID,
                        firstName = volunteer.FirstName,
                        lastName = volunteer.LastName
                    });
                }
            }

            return Json(new { found = false });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Volunteer model)
        {

            if (ModelState.IsValid)
            {
                Guid uniqueId = Guid.NewGuid();
                byte[] bytes = uniqueId.ToByteArray();

                int generatedID = BitConverter.ToInt32(bytes, 0);
                generatedID = Math.Abs(generatedID);

                var volunteer = new Volunteer
                {
                    VolunteerID = generatedID,
                    CreatedDate = DateTime.Now,
                    LastUpdateDate = DateTime.Now,
                    IsDeleted = false,
                    IsActive = false,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    MobileNumber = model.MobileNumber,
                    BirthDate = model.BirthDate,
                    Gender = model.Gender,
                    Country = model.Country,
                    PROV_CODE = model.PROV_CODE,
                    TOWN_CODE = model.TOWN_CODE,
                    BRGY_CODE = model.BRGY_CODE,
                    YearStarted = model.YearStarted,
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

        // [HttpPost]
        // public IActionResult Login(Volunteer model)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         var user = _db.T_Volunteer
        //             .FirstOrDefault(v => v.Email == model.Email && v.Password == model.Password);

        //         if (user != null)
        //         {
        //             return RedirectToAction("Dashboard", "Home");
        //         }
        //         else
        //         {
        //             ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        //         }
        //     }
        //     return View(model);
        // }
    }
}
