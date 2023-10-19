using CSSWENGxGK.Data;
using Microsoft.AspNetCore.Mvc;

namespace CSSWENGxGK.Controllers;
public class VolunteerController : Controller
{
    private readonly ApplicationDbContext _db;

    public IActionResult Register()
    {
        return View();
    }
}
