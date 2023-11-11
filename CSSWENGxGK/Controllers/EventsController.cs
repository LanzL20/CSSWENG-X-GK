using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;

namespace CSSWENGxGK.Controllers;
public class EventsController : Controller
{
	private readonly ApplicationDbContext _db;
	string connectionString = "Server=DESKTOP-SERVS0D;Database=cssweng;Trusted_Connection=True;TrustServerCertificate=True;";
    public EventsController(ApplicationDbContext db)
	{
		_db = db;
	}

    private int MapEventStatus(string EventStatus)
    {
        if (EventStatus == "Ongoing")
        {
            return 0;
        }
        else if (EventStatus == "Finished")
        {
            return 1;
        }
        else if (EventStatus == "Canceled")
        {
            return 2;
        }
        return -1;
    }

    private string ReadEventStatus(int EventStatus)
    {
        if (EventStatus == 0)
        {
            return "Ongoing";
        }
        else if (EventStatus == 1)
        {
            return "Finished";
        }
        else if (EventStatus == 2)
        {
            return "Canceled";
        }
        return "Not Found";
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

    public IActionResult EditOneEvent(int EventID)
    {
        if (EventID > 0)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Fetch Event details
                string eventQuery = $"SELECT EventID, EventName, EventDate, EventLocation, EventShortDesc, EventLongDesc, EventStatus " +
                                   $"FROM T_Event WHERE EventID = {EventID}";

                using (SqlCommand eventCommand = new SqlCommand(eventQuery, connection))
                {
                    using (SqlDataReader eventReader = eventCommand.ExecuteReader())
                    {
                        if (eventReader.Read())
                        {
                            ViewBag.EventID = eventReader["EventID"];
                            ViewBag.EventName = eventReader["EventName"];
                            ViewBag.EventDate = eventReader["EventDate"];
                            ViewBag.EventLocation = eventReader["EventLocation"];
                            ViewBag.EventShortDesc = eventReader["EventShortDesc"];
                            ViewBag.EventLongDesc = eventReader["EventLongDesc"];
                            ViewBag.EventStatus = ReadEventStatus(Convert.ToInt32(eventReader["EventStatus"]));
                        }
                    }
                }

                // Fetch Organizer details for the corresponding event
                string organizerQuery = $"SELECT Name, PhoneNumber, Email " +
                                        $"FROM T_Organizer WHERE EventID = {EventID}";

                using (SqlCommand organizerCommand = new SqlCommand(organizerQuery, connection))
                {
                    using (SqlDataReader organizerReader = organizerCommand.ExecuteReader())
                    {
                        if (organizerReader.Read())
                        {
                            ViewBag.OrganizerName = organizerReader["Name"];
                            ViewBag.OrganizerPhoneNumber = organizerReader["PhoneNumber"];
                            ViewBag.OrganizerEmail = organizerReader["Email"];
                        }
                    }
                }
            }

            return View(EventID.ToString());
        }
        return RedirectToAction("EditEvent");
    }

    [HttpPost]
    public IActionResult EditEvent(Event updatedEvent, Organizer updatedOrganizer)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // Update Event details
            string updateEventQuery = $"UPDATE T_Event SET EventName = '{updatedEvent.EventName}', " +
                                    $"EventDate = '{updatedEvent.EventDate}', " +
                                    $"EventLocation = '{updatedEvent.EventLocation}', " +
                                    $"EventShortDesc = '{updatedEvent.EventShortDesc}', " +
                                    $"EventLongDesc = '{updatedEvent.EventLongDesc}' " +
                                    $"WHERE EventID = {updatedEvent.EventID}";

            using (SqlCommand eventCommand = new SqlCommand(updateEventQuery, connection))
            {
                eventCommand.ExecuteNonQuery();
            }

            // Update Organizer details for the corresponding event
            string updateOrganizerQuery = $"UPDATE T_Organizer SET Name = '{updatedOrganizer.Name}', " +
                                        $"PhoneNumber = '{updatedOrganizer.PhoneNumber}', " +
                                        $"Email = '{updatedOrganizer.Email}' " +
                                        $"WHERE EventID = {updatedEvent.EventID}";

            using (SqlCommand organizerCommand = new SqlCommand(updateOrganizerQuery, connection))
            {
                organizerCommand.ExecuteNonQuery();
            }
        }

        return View("AllEvents");
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
