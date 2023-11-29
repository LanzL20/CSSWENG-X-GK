using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Identity;

namespace CSSWENGxGK.Controllers;
public class EventsController : Controller
{
	private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;
    string connectionString = "Server=localhost\\SQLEXPRESS;Database=cssweng;Trusted_Connection=True;TrustServerCertificate=True;";
    Emailer emailer = new Emailer();

    public EventsController(ApplicationDbContext db, UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
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

    public async Task<IActionResult> AllEvents(string searchQuery, string sortOrder, int pageNumber = 1)
    {
        int? userIdNullable = HttpContext.Session.GetInt32("User_ID");
        string userIdString = userIdNullable?.ToString() ?? "0";

        var user = await _userManager.FindByNameAsync(userIdString);

        // Store current sort parameter and search query in TempData
        TempData["SortOrder"] = sortOrder ?? "";
        TempData["SearchQuery"] = searchQuery ?? "";

        // Query the database to retrieve the total number of events
        IQueryable<Event> eventsQuery = _db.T_Event;

        // Check if the user is an admin
        bool isAdmin = false;
        if (user != null) {
            isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        }

        // Apply search filter if a search query is provided
        if (!string.IsNullOrEmpty(searchQuery))
        {
            eventsQuery = eventsQuery.Where(e =>
                e.EventName.Contains(searchQuery) ||
                e.EventShortDesc.Contains(searchQuery));
        }

        // Apply filtering based on user role
        if (!isAdmin)
        {
            // For non-admin users, only show events with EventStatus = 0
            eventsQuery = eventsQuery.Where(e => e.EventStatus == 0);
        }

        // Apply sorting based on sortOrder parameter
        switch (sortOrder)
        {
            case "date":
                eventsQuery = eventsQuery.OrderBy(e => e.EventDate);
                break;
            case "date_created":
                eventsQuery = eventsQuery.OrderByDescending(e => e.EventID);
                break;
            default:
                // Default sorting: Most recent events first
                eventsQuery = eventsQuery.OrderByDescending(e => e.EventID);
                break;
        }

        int totalEvents = eventsQuery.Count();

        // Query the database to retrieve the events in the specified range
        var events = eventsQuery.ToList();

        ViewData["SearchQuery"] = TempData["SearchQuery"]; // Retrieve searchQuery from TempData

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

    public IActionResult PastOneEvent(string eventId)
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
        return RedirectToAction("AllEvents");
    }

    public IActionResult AddEvent()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddEvent(EventOrganizers model, IFormFile EventImage)
    {
        try
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
                    "INSERT INTO T_Event (EventID, EventName, EventDate, EventLocation, EventShortDesc, EventLongDesc, EventStatus, EventImage, EventEndDate) " +
                    "VALUES (@GeneratedID, @EventName, @EventDate, @EventLocation, @EventShortDesc, @EventLongDesc, @EventStatus, @EventImage, @EventEndDate);" +
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
                    command.Parameters.AddWithValue("@EventEndDate", model.Event.EventEndDate);

                    command.ExecuteNonQuery();
                }

                int updatedOrganizerCount = model.Organizers.Count;

                for (int i = 0; i < updatedOrganizerCount; i++)
                {
					if (model.Organizers[i].Name != null && model.Organizers[i].Email != null && model.Organizers[i].PhoneNumber != null)
                    {
                        string insertOrganizerQuery = "SET IDENTITY_INSERT T_Organizer ON;" +
                                                    "INSERT INTO T_Organizer (OrganizerID, EventID, Name, PhoneNumber, Email) " +
                                                    "VALUES (@newOrgID, @GeneratedID, @Name, @PhoneNumber, @Email);" +
                                                    "SET IDENTITY_INSERT T_Organizer OFF;";

                        using (SqlCommand command = new SqlCommand(insertOrganizerQuery, connection))
                        {
                           var organizers = _db.T_Organizer.Select(d => new Organizer
                           {
                               OrganizerID = d.OrganizerID
                           }).ToList();
                           int newOrgID = organizers.Any() ? organizers.Max(d => d.OrganizerID) + 1 : 1;
    
                           command.Parameters.AddWithValue("@newOrgID", newOrgID);
                           command.Parameters.AddWithValue("@GeneratedID", generatedID);
                            command.Parameters.AddWithValue("@Name", model.Organizers[i].Name);
                           command.Parameters.AddWithValue("@PhoneNumber", model.Organizers[i].PhoneNumber);
                            command.Parameters.AddWithValue("@Email", model.Organizers[i].Email);

                            command.ExecuteNonQuery();
                        }
                    }
                }
						

                List<string> bccRecipients = _db.T_Volunteer
                    .Where(v => v.IsActive && v.IsNotify)
                    .Select(v => v.Email)
                    .ToList();

                bool emailSent = emailer.Send_Notif_Email(bccRecipients, model.Event);

                if (emailSent)
                {
                    Console.WriteLine("Bcc email sent successfully to all recipients");
                }
                else
                {
                    Console.WriteLine("Failed to send Bcc email");
                }
            }

            Console.WriteLine(generatedID);

            return RedirectToAction("AllEvents");
        }
        catch (Exception ex)
        {
            // Handle exceptions
            Console.WriteLine($"Error adding event: {ex.Message}");
            return RedirectToAction("ErrorPage");
        }
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
            List<Organizer> organizers = new List<Organizer>(); // Create a list to store organizer details

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Fetch Event details
                string eventQuery = $"SELECT EventID, EventName, EventDate, EventLocation, EventShortDesc, EventLongDesc, EventStatus, EventImage, EventEndDate " +
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
                            ViewBag.EventEndDate = eventReader["EventEndDate"];
                        }
                    }
                }

                // Fetch Organizer details for the corresponding event
                string organizerQuery = $"SELECT Name, PhoneNumber, Email " +
                                        $"FROM T_Organizer WHERE EventID = {selectedEvent}";

                using (SqlCommand organizerCommand = new SqlCommand(organizerQuery, connection))
                {
                    using (SqlDataReader organizerReader = organizerCommand.ExecuteReader())
                    {
                        while (organizerReader.Read())
                        {
                            // Create Organizer objects and add them to the list
                            Organizer organizer = new Organizer
                            {
                                Name = organizerReader["Name"].ToString(),
                                PhoneNumber = organizerReader["PhoneNumber"].ToString(),
                                Email = organizerReader["Email"].ToString()
                            };
                            organizers.Add(organizer);
                        }
                    }
                }

                // Add the list of organizers to ViewBag
                ViewBag.Organizers = organizers;
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
            DateTime eventEndDate = updatedEvent.Event.EventEndDate;
            string updateEventQuery = "UPDATE T_Event SET EventName = @EventName, EventDate = @EventDate, EventLocation = @EventLocation, EventShortDesc = @EventShortDesc, EventLongDesc = @EventLongDesc, EventStatus = @EventStatus, EventImage = @EventImage, EventEndDate = @EventEndDate WHERE EventID = @EventID";
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
            if (eventEndDate <= DateTime.MinValue || eventEndDate >= DateTime.MaxValue || eventEndDate < eventDate)
            {
                string eventQuery = $"SELECT EventEndDate " +
                                       $"FROM T_Event WHERE EventID = {updatedEvent.Event.EventID}";
                using (SqlCommand eventCommand = new SqlCommand(eventQuery, connection))
                {
                    using (SqlDataReader eventReader = eventCommand.ExecuteReader())
                    {
                        if (eventReader.Read())
                        {
                            eventEndDate = (DateTime)eventReader["EventEndDate"];
                        }

                        Console.WriteLine(eventEndDate);
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
                eventCommand.Parameters.AddWithValue("@EventEndDate", eventEndDate);
                Console.WriteLine("EventStatus:");
                Console.WriteLine(updatedEvent.Event.EventStatus);
				eventCommand.ExecuteNonQuery();
			}

            // Update Organizer details for the corresponding event
            int updatedOrganizerCount = updatedEvent.Organizers.Count;

            int currentOrganizers = -1;

            string countOrganizerTable = "SELECT COUNT(*) AS TotalOrganizers FROM T_Organizer WHERE EventID = @EventID";

            using (SqlCommand command = new SqlCommand(countOrganizerTable, connection))
            {
                command.Parameters.AddWithValue("@EventID", updatedEvent.Event.EventID);

                object result = command.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    currentOrganizers = Convert.ToInt32(result);
                }
            }

            Console.WriteLine("Organizer Counts:");
            Console.WriteLine(currentOrganizers);
            Console.WriteLine(updatedOrganizerCount);

            Console.WriteLine ("\nOrganizers:");
            Console.WriteLine("");

            List<int> organizerIds = new List<int>();

            
            // Fetch Organizer IDs for the corresponding event
            string organizerIdsQuery = $"SELECT OrganizerID FROM T_Organizer WHERE EventID = {updatedEvent.Event.EventID}";

            using (SqlCommand organizerIdsCommand = new SqlCommand(organizerIdsQuery, connection))
            {
                using (SqlDataReader organizerIdsReader = organizerIdsCommand.ExecuteReader())
                {
                    while (organizerIdsReader.Read())
                    {
                        int organizerId = organizerIdsReader.GetInt32(0);
                        organizerIds.Add(organizerId);
                    }
                }
            }

            int j = 0;
            String OrganizerName = "NULL";
            String OrganizerEmail = "NULL";
            String OrganizerPhoneNum = "NULL";

            for (int i = 0; i < currentOrganizers && j < organizerIds.Count; i++)
            {
				string eventQuery = $"SELECT Name, Email, PhoneNumber " +
									   $"FROM T_Organizer WHERE OrganizerID = {organizerIds[j]}";
				using (SqlCommand eventCommand = new SqlCommand(eventQuery, connection))
				{
					using (SqlDataReader eventReader = eventCommand.ExecuteReader())
					{
						if (eventReader.Read())
						{
                            OrganizerName = eventReader["Name"].ToString();
                            OrganizerEmail = eventReader["Email"].ToString();
							OrganizerPhoneNum = eventReader["PhoneNumber"].ToString();
						}
					}
				}

				if (updatedEvent.Organizers[i].Name != null)
				{
                    OrganizerName = updatedEvent.Organizers[i].Name;
                }
                if (updatedEvent.Organizers[i].Email != null)
                {
					OrganizerEmail = updatedEvent.Organizers[i].Email;
				}
                if (updatedEvent.Organizers[i].PhoneNumber != null)
                {
					OrganizerPhoneNum = updatedEvent.Organizers[i].PhoneNumber;
				}

                Console.WriteLine(i);
                Console.WriteLine(OrganizerName);
                Console.WriteLine(OrganizerEmail);
                Console.WriteLine(OrganizerPhoneNum);

                string updateOrganizerQuery = $"UPDATE T_Organizer SET Name = '{OrganizerName}', " +
                                                  $"PhoneNumber = '{OrganizerPhoneNum}', " +
                                                  $"Email = '{OrganizerEmail}' " +
                                                  $"WHERE OrganizerID = {organizerIds[j]}";
                using (SqlCommand organizerCommand = new SqlCommand(updateOrganizerQuery, connection))
                {
                    organizerCommand.ExecuteNonQuery();
                }
                j++;
            }

            Console.WriteLine("\nNew Organizers:");
            Console.WriteLine("");

            if (updatedOrganizerCount > currentOrganizers)
            {
                for (int i = currentOrganizers; i < updatedOrganizerCount; i++)
                {
					if (updatedEvent.Organizers[i].Name != null && updatedEvent.Organizers[i].Email != null && updatedEvent.Organizers[i].PhoneNumber != null)
					{
						string insertOrganizerQuery = "SET IDENTITY_INSERT T_Organizer ON;" +
                                                    "INSERT INTO T_Organizer (OrganizerID, EventID, Name, PhoneNumber, Email) " +
                                                    "VALUES (@newOrgID, @GeneratedID, @Name, @PhoneNumber, @Email);" +
                                                    "SET IDENTITY_INSERT T_Organizer OFF;";

                        using (SqlCommand command = new SqlCommand(insertOrganizerQuery, connection))
                        {
                            var organizers = _db.T_Organizer.Select(d => new Organizer
                            {
                                OrganizerID = d.OrganizerID
                            }).ToList();
                            Console.WriteLine(i);
                            Console.WriteLine(updatedEvent.Organizers[i].Name);
                            Console.WriteLine(updatedEvent.Organizers[i].Email);
                            Console.WriteLine(updatedEvent.Organizers[i].PhoneNumber);
                            int newOrgID = organizers.Any() ? organizers.Max(d => d.OrganizerID) + 1 : 1;

                            command.Parameters.AddWithValue("@newOrgID", newOrgID);
                            command.Parameters.AddWithValue("@GeneratedID", updatedEvent.Event.EventID);
                            command.Parameters.AddWithValue("@Name", updatedEvent.Organizers[i].Name);
                            command.Parameters.AddWithValue("@PhoneNumber", updatedEvent.Organizers[i].PhoneNumber);
                            command.Parameters.AddWithValue("@Email", updatedEvent.Organizers[i].Email);

                            command.ExecuteNonQuery();
                        }
					}
					
                }
            }
        }

        return RedirectToAction("OneEvent", new {eventId = updatedEvent.Event.EventID});
    }

    public IActionResult PastEvents()
    {
        List<Event> eventInfoList = new List<Event>();

        int? userIdNullable = HttpContext.Session.GetInt32("User_ID");
        int userId = userIdNullable ?? 0;

        var pastEvents = (from ea in _db.T_EventsAttended
                          join e in _db.T_Event on ea.EventID equals e.EventID
                          where ea.VolunteerID == userId
                          orderby e.EventDate descending
                          select new Event
                          {
                              EventID = e.EventID,
                              EventImage = e.EventImage,
                              EventName = e.EventName,
                              EventDate = e.EventDate,
                              EventLocation = e.EventLocation,
                              EventShortDesc = e.EventShortDesc,
                              EventLongDesc = e.EventLongDesc,
                              EventStatus = e.EventStatus
                          }).ToList();

        eventInfoList.AddRange(pastEvents);

        return View(eventInfoList);
    }

}
