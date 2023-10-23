using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Mvc;

namespace CSSWENGxGK.Controllers;
public class VolunteerController : Controller
{
    private readonly ApplicationDbContext _db;

    public VolunteerController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(Volunteer volunteer)
    {
        volunteer.CreatedDate = DateTime.Now;
        volunteer.LastUpdateDate = DateTime.Now;
        volunteer.IsDeleted = false;
        volunteer.IsActive = false;

        if (ModelState.IsValid) {
            _db.T_Volunteer.Add(volunteer);
            _db.SaveChanges();
            return RedirectToAction("Register");
        }
        return View(volunteer);
    }
}
