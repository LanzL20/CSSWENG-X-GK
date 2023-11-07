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

    public IActionResult AllEvents(int pageNumber = 1)
    {
        pageNumber = HttpContext.Session.GetInt32("Page_number") ?? 1;
        int pageSize = 9; // Number of events per page

        // Calculate the start and end index for the events to retrieve
        int startIndex = (pageNumber - 1) * pageSize;
        int endIndex = pageNumber * pageSize;

        // Query the database to retrieve the events in the specified range
        var events = _db.T_Event
            .OrderBy(e => e.EventID)
            .Skip(startIndex)
            .Take(pageSize)
            .Select(e => new Event
            {
                EventID = e.EventID, // Store the EventID
                EventName = e.EventName,
                EventShortDesc = e.EventShortDesc
            })
            .ToList();

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
        int selectedEvent = -1;

        selectedEvent = HttpContext.Session.GetInt32("Selected_event") ?? -1;

        if (selectedEvent > 0)
        {
            // Retrieve the event name based on the current event ID (selectedEvent) and store it in ViewBag
            var eventInfo = _db.T_Event.FirstOrDefault(e => e.EventID == selectedEvent);

            // Retrieve the VolunteerIDs that joined the selected event
            var volunteerIds = _db.T_EventsAttended
                .Where(e => e.EventID == selectedEvent)
                .Select(e => e.VolunteerID)
                .ToList();

            // Retrieve all volunteer information for the volunteer IDs
            var volunteersInfo = _db.T_Volunteer
                .Where(volunteer => volunteerIds.Contains(volunteer.VolunteerID))
                .Select(volunteer => new
                {
                    VolunteerID = volunteer.VolunteerID,
                    FirstName = volunteer.FirstName,
                    LastName = volunteer.LastName
                })
                .ToList();

            // Store the volunteersInfo list in ViewBag
            ViewBag.EventInfo = eventInfo;
            ViewBag.volunteersInfo = volunteersInfo;

            // You can access ViewBag.volunteersInfo in your view
            return View();
        }
        else
        {
            // Handle the case where the selectedEvent is invalid
            return View("InvalidEvent");
        }


        return View();
    }

    public IActionResult EditOneEvent(){
        return View();
    }
}
