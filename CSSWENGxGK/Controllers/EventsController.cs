using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Mvc;

namespace CSSWENGxGK.Controllers;
public class EventsController : Controller
{
    private readonly ApplicationDbContext _db;
    string connectionString = "Server=DESKTOP-SERVS0D;Database=cssweng;Trusted_Connection=True;TrustServerCertificate=True;";

    public EventsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult AllEvents()
    {
        // Query the database to retrieve a list of all events
        var events = _db.T_Event.ToList();

        // Pass the list of events to the view
        return View(events);
    }

    public IActionResult OneEvent(string eventId)
    {
        int selectedEvent = -1;

        if (int.TryParse(eventId, out int eventIdValue))
        {
            HttpContext.Session.SetInt32("Selected_event", eventIdValue);
            selectedEvent = HttpContext.Session.GetInt32("Selected_event") ?? -1;
        }

        Console.WriteLine(selectedEvent);
        // Check if the selectedEvent is valid (greater than 0) before querying the database
        if (selectedEvent > 0)
        {
            // Query the database to get event information based on selectedEvent
            var eventInfo = _db.T_Event.FirstOrDefault(e => e.EventID == selectedEvent);

            if (eventInfo != null)
            {
                // Query the organizers for the event
                var organizers = _db.T_Organizer.Where(o => o.EventID == selectedEvent).ToList();

                // Store the eventInfo and organizers in ViewBag
                ViewBag.EventInfo = eventInfo;
                ViewBag.Organizers = organizers;

                return View(); // Pass the ViewModel to the view
            }
            else
            {
                // Handle the case where the event is not found in the database
                return View("EventNotFound");
            }
        }
        else
        {
            // Handle the case where the selectedEvent is invalid
            return View("InvalidEvent");
        }
    }

    public IActionResult VolunteerList()
    {
        return View();
    }
}
