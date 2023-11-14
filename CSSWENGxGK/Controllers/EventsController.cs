using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System;
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
        int pageSize = 9; // Number of events per page

        // Ensure that pageNumber is at least 1
        pageNumber = HttpContext.Session.GetInt32("Page_number") ?? 1; ;

        pageNumber = Math.Max(pageNumber, 1);

        Console.WriteLine(pageNumber);
        // Query the database to retrieve the total number of events
        int totalEvents = _db.T_Event.Count();

        // Calculate the start and end index for the events to retrieve
        int startIndex = (pageNumber - 1) * pageSize;
        int endIndex = pageNumber * pageSize;

        // Query the database to retrieve the events in the specified range
        var events = _db.T_Event
            .OrderByDescending(e => e.EventID)
            .Skip(startIndex)
            .Take(pageSize)
            .Select(e => new Event
            {
                EventID = e.EventID, // Store the EventID
                EventName = e.EventName,
                EventShortDesc = e.EventShortDesc,
                EventImage = e.EventImage
            })
            .ToList();

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

    public IActionResult AddEvent()
    {
        return View();
    }

    public IActionResult EditOneEvent(string eventId)
    {

		int selectedEvent = -1;

		if (int.TryParse(eventId, out int eventIdValue))
		{
			HttpContext.Session.SetInt32("Selected_event", eventIdValue);
			selectedEvent = HttpContext.Session.GetInt32("Selected_event") ?? -1;
		}

		Console.WriteLine(selectedEvent);

		if (selectedEvent > 0)
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Fetch Event details
                string eventQuery = $"SELECT EventID, EventName, EventDate, EventLocation, EventShortDesc, EventLongDesc, EventStatus, EventImage " +
                                   $"FROM T_Event WHERE EventID = {selectedEvent}";

                using (SqlCommand eventCommand = new SqlCommand(eventQuery, connection))
                {
                    using (SqlDataReader eventReader = eventCommand.ExecuteReader())
                    {
                        if (eventReader.Read())
                        {
                            ViewBag.EventID = selectedEvent;
                            ViewBag.EventName = eventReader["EventName"];
                            ViewBag.EventDate = eventReader["EventDate"];
                            ViewBag.EventLocation = eventReader["EventLocation"];
                            ViewBag.EventShortDesc = eventReader["EventShortDesc"];
                            ViewBag.EventLongDesc = eventReader["EventLongDesc"];
                            ViewBag.EventStatus = eventReader["EventStatus"];
                            ViewBag.EventStatusString = ReadEventStatus(Convert.ToInt32(eventReader["EventStatus"]));
                            ViewBag.EventImage = eventReader["EventImage"];

                            Console.WriteLine(ViewBag.EventStatus);
                        }

                        Console.WriteLine(ViewBag.EventDate);
                    }
                }

                // Fetch Organizer details for the corresponding event
                string organizerQuery = $"SELECT Name, PhoneNumber, Email " +
                                        $"FROM T_Organizer WHERE EventID = {selectedEvent}";

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

            return View();
        }
        return View();
    }

    [HttpPost]
    public IActionResult EditEvent(EventOrganizers updatedEvent, IFormFile EventImage)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            DateTime eventDate = updatedEvent.Event.EventDate;
			string updateEventQuery = "UPDATE T_Event SET EventName = @EventName, EventDate = @EventDate, EventLocation = @EventLocation, EventShortDesc = @EventShortDesc, EventLongDesc = @EventLongDesc, EventStatus = @EventStatus, EventImage = @EventImage WHERE EventID = @EventID";
            Console.WriteLine(eventDate);
            Console.WriteLine(DateTime.MinValue);
            Console.WriteLine(eventDate <= DateTime.MinValue);

            if (eventDate <= DateTime.MinValue || eventDate >= DateTime.MaxValue)
            {
                string eventQuery = $"SELECT EventDate " +
                                       $"FROM T_Event WHERE EventID = {updatedEvent.Event.EventID}";
                using (SqlCommand eventCommand = new SqlCommand(eventQuery, connection))
                {
                    using (SqlDataReader eventReader = eventCommand.ExecuteReader())
                    {
                        if (eventReader.Read())
                        {
                            eventDate = (DateTime)eventReader["EventDate"];
                        }

                        Console.WriteLine(eventDate);
                    }
                }
            }
            if (EventImage != null)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    EventImage.CopyTo(memoryStream);
                    updatedEvent.Event.EventImage = memoryStream.ToArray(); // Convert the uploaded image to byte array
                }
            }
            else
            {
                string eventQuery = $"SELECT EventImage " +
                                           $"FROM T_Event WHERE EventID = {updatedEvent.Event.EventID}";
                using (SqlCommand eventCommand = new SqlCommand(eventQuery, connection))
                {
                    using (SqlDataReader eventReader = eventCommand.ExecuteReader())
                    {
                        if (eventReader.Read())
                        {
                            updatedEvent.Event.EventImage = (byte[])eventReader["EventImage"];
                        }
                    }
                }
            }


            using (SqlCommand eventCommand = new SqlCommand(updateEventQuery, connection))
			{
				eventCommand.Parameters.AddWithValue("@EventName", updatedEvent.Event.EventName);
				eventCommand.Parameters.AddWithValue("@EventDate", eventDate);
				eventCommand.Parameters.AddWithValue("@EventLocation", updatedEvent.Event.EventLocation);
				eventCommand.Parameters.AddWithValue("@EventShortDesc", updatedEvent.Event.EventShortDesc);
				eventCommand.Parameters.AddWithValue("@EventLongDesc", updatedEvent.Event.EventLongDesc);
                eventCommand.Parameters.AddWithValue("@EventStatus", updatedEvent.Event.EventStatus);
                eventCommand.Parameters.AddWithValue("@EventID", updatedEvent.Event.EventID);
                eventCommand.Parameters.AddWithValue("@EventImage", updatedEvent.Event.EventImage);
                Console.WriteLine("EventStatus:");
                Console.WriteLine(updatedEvent.Event.EventStatus);
				eventCommand.ExecuteNonQuery();
			}

            // Update Organizer details for the corresponding event
            string updateOrganizerQuery = $"UPDATE T_Organizer SET Name = '{updatedEvent.Organizers[0].Name}', " +
                                        $"PhoneNumber = '{updatedEvent.Organizers[0].PhoneNumber}', " +
                                        $"Email = '{updatedEvent.Organizers[0].Email}' " +
                                        $"WHERE EventID = {updatedEvent.Event.EventID}";

            Console.WriteLine ("Organizers:");
            Console.WriteLine(updatedEvent.Organizers[0].Email);
            Console.WriteLine(updatedEvent.Organizers[0].PhoneNumber);

            Console.WriteLine(updatedEvent.Event.EventName);
            Console.WriteLine("EventID:");
            Console.WriteLine(updatedEvent.Event.EventID);

            using (SqlCommand organizerCommand = new SqlCommand(updateOrganizerQuery, connection))
            {
                organizerCommand.ExecuteNonQuery();
            }
        }

        return RedirectToAction("OneEvent", new {eventId = updatedEvent.Event.EventID});
    }

    public IActionResult DeleteEvent(int EventID)
    {
        if (EventID > 0)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Delete Organizer details for the corresponding event
                    string deleteOrganizerQuery = "DELETE FROM T_Organizer WHERE EventID = @EventID";

                    using (SqlCommand deleteOrganizerCommand = new SqlCommand(deleteOrganizerQuery, connection, transaction))
                    {
                        deleteOrganizerCommand.Parameters.AddWithValue("@EventID", EventID);
                        deleteOrganizerCommand.ExecuteNonQuery();
                    }

                    // Delete Event details
                    string deleteEventQuery = "DELETE FROM T_Event WHERE EventID = @EventID";

                    using (SqlCommand deleteEventCommand = new SqlCommand(deleteEventQuery, connection, transaction))
                    {
                        deleteEventCommand.Parameters.AddWithValue("@EventID", EventID);
                        deleteEventCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return RedirectToAction("AllEvents");
                }
                catch (Exception ex)
                {
                    // Roll back the transaction in case of an error
                    transaction.Rollback();
                }
            }
        }
        return RedirectToAction("EditEvent");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddEvent(EventOrganizers model, IFormFile EventImage)
    {
        if (EventImage != null)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                EventImage.CopyTo(memoryStream);
                model.Event.EventImage = memoryStream.ToArray(); // Convert the uploaded image to byte array
            }
        }

        var events = _db.T_Event.Select(e => new Event
        {
            EventID = e.EventID
        }).ToList();

        int generatedID = events.Any() ? events.Max(e => e.EventID) + 1 : 1;

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string query = "SET IDENTITY_INSERT T_Event ON;" +
                "INSERT INTO T_Event (EventID, EventName, EventDate, EventLocation, EventShortDesc, EventLongDesc, EventStatus, EventImage) " +
                "VALUES (@GeneratedID, @EventName, @EventDate, @EventLocation, @EventShortDesc, @EventLongDesc, @EventStatus, @EventImage);" +
                "SET IDENTITY_INSERT T_Event OFF;";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@GeneratedID", generatedID);
                command.Parameters.AddWithValue("@EventName", model.Event.EventName);
                command.Parameters.AddWithValue("@EventDate", model.Event.EventDate);
                command.Parameters.AddWithValue("@EventLocation", model.Event.EventLocation);
                command.Parameters.AddWithValue("@EventShortDesc", model.Event.EventShortDesc);
                command.Parameters.AddWithValue("@EventLongDesc", model.Event.EventLongDesc);
                command.Parameters.AddWithValue("@EventStatus", 0);
                command.Parameters.AddWithValue("@EventImage", model.Event.EventImage); // Assuming EventImage is a byte[]

                command.ExecuteNonQuery();
            }

            string query2 = "SET IDENTITY_INSERT T_Organizer ON;" +
                "INSERT INTO T_Organizer (EventID, Name, PhoneNumber, Email) " +
                "VALUES (@GeneratedID, @Name, @PhoneNumber, @Email);" +
                "SET IDENTITY_INSERT T_Organizer OFF;";

            using (SqlCommand command = new SqlCommand(query2, connection))
            {
                command.Parameters.AddWithValue("@GeneratedID", generatedID);
                command.Parameters.AddWithValue("@Name", model.Organizers[0].Name);
                command.Parameters.AddWithValue("@PhoneNumber", model.Organizers[0].PhoneNumber);
                command.Parameters.AddWithValue("@Email", model.Organizers[0].Email);

                command.ExecuteNonQuery();
            }
        }

        Console.WriteLine(generatedID);

        return RedirectToAction("AllEvents");
    }

    public IActionResult EventPagePrevious()
    {
        // Get the current page number from the session, default to 1 if not set
        int pageNumber = HttpContext.Session.GetInt32("Page_number") ?? 1;

        // Set page_number to max(1, page_number - 1)
        pageNumber = Math.Max(1, pageNumber - 1);

        // Update the session with the new page number
        HttpContext.Session.SetInt32("Page_number", pageNumber);

        return RedirectToAction("AllEvents");
    }

    public IActionResult EventPageNext()
    {
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // SQL command to get the total number of events
            var totalEventsQuery = "SELECT COUNT(*) FROM T_Event";

            using (var command = new SqlCommand(totalEventsQuery, connection))
            {
                int totalEvents = (int)command.ExecuteScalar();

                // Get the current page number from the session, default to 1 if not set
                int pageNumber = HttpContext.Session.GetInt32("Page_number") ?? 1;

                // Calculate the maximum page number based on the total number of events
                int maxPageNumber = (int)Math.Ceiling((double)totalEvents / 9);

                // Set page_number to min(pageNumber + 1, maxPageNumber)
                pageNumber = Math.Min(pageNumber + 1, maxPageNumber);

                // Update the session with the new page number
                HttpContext.Session.SetInt32("Page_number", pageNumber);
            }

            connection.Close();
        }

        return RedirectToAction("AllEvents");
    }

}