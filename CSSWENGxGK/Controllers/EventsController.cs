using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace CSSWENGxGK.Controllers;
public class EventsController : Controller
{
	private readonly ApplicationDbContext _db;
	string connectionString = "Server=FRANCINECHAN\\SQLEXPRESS;Database=cssweng;Trusted_Connection=True;TrustServerCertificate=True;";
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

	public IActionResult BackEvent()
	{
		// Get the selectedEvent from the session
		int selectedEvent = HttpContext.Session.GetInt32("Selected_event") ?? -1;
		return RedirectToAction("OneEvent", new { eventId = selectedEvent });
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

	public IActionResult EditOneEvent()
	{
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult AddEvent(Event model)
	{
		if (ModelState.IsValid)
        {
            var events = _db.T_Event.Select(e => new Event
            {
                EventID = e.EventID
            }).ToList();

            int generatedID = events.Any() ? events.Max(e => e.EventID) + 1 : 1;

            // Define the SQL insert query
            string query = "SET IDENTITY_INSERT T_Event ON;" +
						  "INSERT INTO T_Event (EventID, EventName, EventDate, EventLocation, EventShortDesc, EventLongDesc, EventStatus) " +
						  "VALUES (@GeneratedID, @EventName, @EventDate, @EventLocation, @EventShortDesc, @EventLongDesc, @EventStatus);" +
						  "SET IDENTITY_INSERT T_Event OFF;";

            using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@GeneratedID", generatedID);
					command.Parameters.AddWithValue("@EventName", model.EventName);
					command.Parameters.AddWithValue("@EventDate", model.EventDate);
					command.Parameters.AddWithValue("@EventLocation", model.EventLocation);
					command.Parameters.AddWithValue("@EventShortDesc", model.EventShortDesc);
					command.Parameters.AddWithValue("@EventLongDesc", model.EventLongDesc);
					command.Parameters.AddWithValue("@EventStatus", 0);
                }
			}
        }

		return View("../Home/Index");
	}
}