using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Mvc;

namespace CSSWENGxGK.Controllers;
public class EventsController : Controller
{
    public IActionResult Display()
    {
        return View();
    }
}
