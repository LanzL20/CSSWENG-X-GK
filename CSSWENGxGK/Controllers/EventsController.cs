using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Mvc;

namespace CSSWENGxGK.Controllers;
public class EventsController : Controller
{
    public IActionResult AllEvents()
    {
        return View();
    }

    public IActionResult OneEvent(string eventId)
    {
        if (int.TryParse(eventId, out int eventIdValue))
        {
            HttpContext.Session.SetInt32("Selected_event", eventIdValue);
            int selectedEvent = HttpContext.Session.GetInt32("Selected_event") ?? -1;
        }
        return View();
    }

    public IActionResult VolunteerList()
    {
        return View();
    }
}
