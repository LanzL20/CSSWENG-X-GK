using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Data.SqlClient;
using BCrypt.Net;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZXing;
using ZXing.Windows.Compatibility;

namespace CSSWENGxGK.Controllers
{

    public class VolunteerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        Emailer emailer = new Emailer();
        string connectionString = "Server=DESKTOP-SERVS0D;Database=cssweng;Trusted_Connection=True;TrustServerCertificate=True;";

        public VolunteerController(ApplicationDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Login()
        {
            // Retrieve the value of "MyCookie" from the request's cookies
            if (HttpContext.Request.Cookies.TryGetValue("MyCookie", out string cookieValue))
            {
                // Check if the "UserID" value is not null or empty
                if (!string.IsNullOrEmpty(cookieValue) && int.TryParse(cookieValue, out int userId))
                {
                    // Store the user ID in the session
                    HttpContext.Session.SetInt32("User_ID", userId);
                    return RedirectToAction("Profile");
                }
            }

            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
        
        public static string GenerateQRCode(string text, int size = 300)
        {
            var barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.QR_CODE;

            // Encode the text as a QR code
            var qrCodeBitmap = barcodeWriter.Write(text);

            // Set the size of the QR code image
            qrCodeBitmap = new Bitmap(qrCodeBitmap, new Size(size, size));

            // Create a new bitmap without the border
            var borderlessQRCodeBitmap = new Bitmap(qrCodeBitmap.Width, qrCodeBitmap.Height);
            using (Graphics g = Graphics.FromImage(borderlessQRCodeBitmap))
            {
                g.Clear(Color.White);
                g.DrawImage(qrCodeBitmap, Point.Empty);
            }

            // Convert the borderless QR code bitmap to a byte array
            byte[] qrCodeBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                borderlessQRCodeBitmap.Save(ms, ImageFormat.Png);
                qrCodeBytes = ms.ToArray();
            }

            // Convert the byte array to a base64-encoded data URI
            string dataUri = "data:image/png;base64," + Convert.ToBase64String(qrCodeBytes);

            return dataUri;
        }

        public IActionResult Sign_Out()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("MyCookie");
            return RedirectToAction("Login");
        }

        public IActionResult Profile()
        {
            int? userIdNullable = HttpContext.Session.GetInt32("User_ID");
            int userId = userIdNullable ?? 0;

            // Check if the user is logged in and has a valid userId
            if (userId != 0)
            {
                string userIdString = userId.ToString();
                string qrCodeImageUrl = GenerateQRCode(userIdString);

                ViewBag.QRCodeImageUrl = qrCodeImageUrl;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT VolunteerID, FirstName, LastName, Email, MobileNumber, BirthDate, Gender, Country, PROV_CODE, TOWN_CODE, BRGY_CODE, YearStarted, CreatedDate, LastUpdateDate, IsNotify FROM T_Volunteer WHERE VolunteerID = @parsedVolunteerID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@parsedVolunteerID", userId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // The volunteer was found in the database
                                ViewBag.VolunteerID = reader["VolunteerID"];
                                ViewBag.FirstName = reader["FirstName"];
                                ViewBag.LastName = reader["LastName"];
                                ViewBag.Email = reader["Email"];
                                ViewBag.MobileNumber = reader["MobileNumber"];
                                ViewBag.BirthDate = reader["BirthDate"];
                                ViewBag.Gender = reader["Gender"];
                                ViewBag.Country = reader["Country"];
                                ViewBag.Province = reader["PROV_CODE"];
                                ViewBag.Town = reader["TOWN_CODE"];
                                ViewBag.Barangay = reader["BRGY_CODE"];
                                ViewBag.YearStarted = reader["YearStarted"];
                                ViewBag.CreatedDate = reader["CreatedDate"];
                                ViewBag.LastUpdateDate = reader["LastUpdateDate"];
                                ViewBag.IsNotify = reader["IsNotify"];
                            }
                        }
                    }
                }
                return View();
            }

            return RedirectToAction("Login");
        }

        private bool EmailExists(string email)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM T_Volunteer WHERE Email = @Email";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    int emailCount = (int)command.ExecuteScalar();
                    return emailCount > 0;
                }
            }
        }

        //Fix this function cant change to same email
        [HttpPost]
        public IActionResult Save_Profile_Changes(Volunteer model)
        {
            int? userIdNullable = HttpContext.Session.GetInt32("User_ID");
            int userId = userIdNullable ?? 0;


            if (userId != 0)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Email FROM T_Volunteer WHERE VolunteerID = @parsedVolunteerID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {

                        command.Parameters.AddWithValue("@parsedVolunteerID", userId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // The volunteer was found in the database
                                var current_Email = reader["Email"];

                                reader.Close();

                                if (!EmailExists(model.Email) || model.Email.Equals(current_Email))
                                {

                                    string updateQuery = "UPDATE T_Volunteer SET FirstName = @FirstName, LastName = @LastName, Email = @Email, MobileNumber = @MobileNumber, BirthDate = @BirthDate, Gender = @Gender, Country = @Country, PROV_CODE = @PROV_CODE, TOWN_CODE = @TOWN_CODE, BRGY_CODE = @BRGY_CODE, YearStarted = @YearStarted, IsNotify = @IsNotify WHERE VolunteerID = @VolunteerID";

                                    using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                                    {
                                        updateCommand.Parameters.AddWithValue("@FirstName", model.FirstName);
                                        updateCommand.Parameters.AddWithValue("@LastName", model.LastName);
                                        updateCommand.Parameters.AddWithValue("@Email", model.Email);
                                        updateCommand.Parameters.AddWithValue("@MobileNumber", model.MobileNumber);
                                        updateCommand.Parameters.AddWithValue("@BirthDate", model.BirthDate);
                                        updateCommand.Parameters.AddWithValue("@Gender", model.Gender);
                                        updateCommand.Parameters.AddWithValue("@Country", model.Country);
                                        updateCommand.Parameters.AddWithValue("@PROV_CODE", model.PROV_CODE);
                                        updateCommand.Parameters.AddWithValue("@TOWN_CODE", model.TOWN_CODE);
                                        updateCommand.Parameters.AddWithValue("@BRGY_CODE", model.BRGY_CODE);
                                        updateCommand.Parameters.AddWithValue("@YearStarted", model.YearStarted);
                                        updateCommand.Parameters.AddWithValue("@IsNotify", model.IsNotify);
                                        Console.WriteLine(model.IsNotify);
                                        updateCommand.Parameters.AddWithValue("@VolunteerID", userId);

                                        try
                                        {
                                            int rowsAffected = updateCommand.ExecuteNonQuery();

                                            if (rowsAffected > 0)
                                            {
                                                // The update was successful, and rows were modified
                                                Console.WriteLine("ACCEPED");
                                            }
                                            else
                                            {
                                                // The update was successful, but no rows were modified (likely because the data is unchanged)
                                                Console.WriteLine("No changes were made");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // Handle the exception, e.g., log or display an error message
                                            Console.WriteLine("Error: " + ex.Message);
                                        }
                                    }
                                }
                                else
                                {
                                    ViewBag.Message = "Email already exists. Please choose a different email.";
                                    Console.WriteLine("Email conflict");
                                }
                            }
                        }
                    }
                }
            }
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> VerifyVolunteerID(string Vol_ID)
        {
            if (int.TryParse(Vol_ID, out int parsedVolunteerID) && parsedVolunteerID > 0)
            {

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        string query = "SELECT VolunteerID, FirstName, LastName FROM T_Volunteer WHERE VolunteerID = @parsedVolunteerID";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {

                            command.Parameters.AddWithValue("@parsedVolunteerID", parsedVolunteerID);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    // The volunteer was found in the database
                                    var volunteerID = reader["VolunteerID"];
                                    var firstName = reader["FirstName"];
                                    var lastName = reader["LastName"];

                                    Successful_Volunteer newVolunteer = new Successful_Volunteer
                                    {
                                        found = true,
                                        volunteerID = (int)volunteerID,
                                        firstName = (string)firstName,
                                        lastName = (string)lastName
                                    };


                                    reader.Close();

                                    DateTime timeIn = DateTime.Now;
                                    DateTime? timeOut = null;
                                    int selectedEvent = HttpContext.Session.GetInt32("Selected_event") ?? -1;

                                    // Insert the record into the T_EventsAttended table
                                    string insertQuery = "INSERT INTO T_EventsAttended (VolunteerID, EventID, TimeIn, TimeOut) " + "VALUES (@VolunteerID, @EventID, @TimeIn, @TimeOut)";

                                    try
                                    {
                                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                                        {
                                            Console.WriteLine("HERE");

                                            insertCommand.Parameters.AddWithValue("@VolunteerID", parsedVolunteerID);
                                            insertCommand.Parameters.AddWithValue("@EventID", selectedEvent);
                                            insertCommand.Parameters.AddWithValue("@TimeIn", timeIn);
                                            insertCommand.Parameters.AddWithValue("@TimeOut", DBNull.Value);

                                            int rowsAffected = insertCommand.ExecuteNonQuery();

                                            if (rowsAffected > 0)
                                            {
                                                // Log a success message
                                                System.Diagnostics.Trace.WriteLine("INSERT operation was successful.");
                                            }
                                            else
                                            {
                                                // Log a failure message
                                                System.Diagnostics.Trace.WriteLine("INSERT operation did not affect any rows.");
                                            }
                                            Console.WriteLine("OK");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex);
                                        // Log an error message if an exception occurs
                                        System.Diagnostics.Trace.WriteLine("Error during INSERT operation: " + ex.Message);
                                    }

                                    return Json(newVolunteer);
                                }
                                else
                                {
                                    // Volunteer not found
                                    return Json(new Successful_Volunteer
                                    {
                                        found = false,
                                        volunteerID = 0,
                                        firstName = "Volunteer",
                                        lastName = "Not Found"
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle the exception, log it, or return a more meaningful error message
                        return Json(new Successful_Volunteer
                        {
                            found = false,
                            volunteerID = 0,
                            firstName = "Error",
                            lastName = ex.Message
                        });
                    }
                }
                // Close the connection and dispose of resources here
            }
            else
            {
                // Invalid volunteer ID
                return Json(new Successful_Volunteer
                {
                    found = false,
                    volunteerID = 0,
                    firstName = "Invalid",
                    lastName = "Volunteer ID"
                });
            }
        }

        //add verification like duplciate emails and other things
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Volunteer model)
        {
            if (ModelState.IsValid)
            {
                if (EmailExists(model.Email))
                {
                    return RedirectToAction("Register");
                }

                string query2 = "SELECT COUNT(*) FROM T_Volunteer";

                // Define the SQL insert query
                string query = "SET IDENTITY_INSERT T_Volunteer ON;" +
                              "INSERT INTO T_Volunteer (VolunteerID, CreatedDate, LastUpdateDate, IsDeleted, IsActive, IsNotify, Password, FirstName, LastName, Email, MobileNumber, BirthDate, Gender, Country, PROV_CODE, TOWN_CODE, BRGY_CODE, YearStarted) " +
                              "VALUES (@GeneratedID, @CreatedDate, @LastUpdateDate, @IsDeleted, @IsActive, @IsNotify, @Password, @FirstName, @LastName, @Email, @MobileNumber, @BirthDate, @Gender, @Country, @PROV_CODE, @TOWN_CODE, @BRGY_CODE, @YearStarted);" +
                              "SET IDENTITY_INSERT T_Volunteer OFF;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    int count = 0;
                    int generatedID = 0;

                    using (SqlCommand command = new SqlCommand(query2, connection))
                    {
                        count = (int)command.ExecuteScalar();
                        generatedID = count + 1;
                    }


                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GeneratedID", generatedID);
                        command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                        command.Parameters.AddWithValue("@LastUpdateDate", DateTime.Now);
                        command.Parameters.AddWithValue("@IsDeleted", false);
                        command.Parameters.AddWithValue("@IsActive", true);
                        command.Parameters.AddWithValue("@FirstName", model.FirstName);
                        command.Parameters.AddWithValue("@LastName", model.LastName);
                        command.Parameters.AddWithValue("@Email", model.Email);
                        command.Parameters.AddWithValue("@MobileNumber", model.MobileNumber);
                        command.Parameters.AddWithValue("@BirthDate", model.BirthDate);
                        command.Parameters.AddWithValue("@Gender", model.Gender);
                        command.Parameters.AddWithValue("@Country", model.Country);

                        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password, 16);
                        command.Parameters.AddWithValue("@Password", hashedPassword);

                        var emailNotificationsEnabled = model.IsNotify;

                        if (emailNotificationsEnabled)
                        {
                            command.Parameters.AddWithValue("@IsNotify", 1);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@IsNotify", 0);
                        }

                        if (model.Country.ToLower() == "philippines")
                        {
                            command.Parameters.AddWithValue("@PROV_CODE", model.PROV_CODE);
                            command.Parameters.AddWithValue("@TOWN_CODE", model.TOWN_CODE);
                            command.Parameters.AddWithValue("@BRGY_CODE", model.BRGY_CODE);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@PROV_CODE", -1);
                            command.Parameters.AddWithValue("@TOWN_CODE", -1);
                            command.Parameters.AddWithValue("@BRGY_CODE", -1);
                        }
                        command.Parameters.AddWithValue("@YearStarted", model.YearStarted);

                        command.ExecuteNonQuery();

                        // change session user
                        HttpContext.Session.SetInt32("User_ID", generatedID);

                        var new_user = new User
                        {
                            user_id = generatedID,
                        };

                        new_user.UserName = generatedID.ToString();
                        new_user.Email = model.Email;

                         //create user and add role
                        await _userManager.CreateAsync(new_user, "Password123!");
                        return RedirectToAction("Profile");
                    }
                }
            }

            return RedirectToAction("Register");
        }


        // find a way to generate user role

        [HttpPost]
        public async Task<IActionResult> Login(LogInfo model)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string email = model.Email;
                    string password = model.Password;
                    bool remember = model.IsRemember;

                    string query = "SELECT Password, VolunteerID, IsActive FROM T_Volunteer WHERE Email = @Email";
                    Console.WriteLine(email);

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        //int OTP = emailer.Send_OTP(email);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool isActive = reader.GetBoolean(reader.GetOrdinal("IsActive"));

                                if (isActive)
                                {
                                    string hashedPassword = reader["Password"].ToString();

                                    if (BCrypt.Net.BCrypt.Verify(password, hashedPassword))
                                    {
                                        // Passwords match; user can be authenticated
                                        if (reader["VolunteerID"] is int volunteerID)
                                        {
                                            // If "VolunteerID" is an integer, assign it to volunteerID
                                            HttpContext.Session.SetInt32("User_ID", volunteerID);
                                        }

                                        // Retrieve the User_ID from the session with a default value of 0 if it's null (for debugging)
                                        int? userIdNullable = HttpContext.Session.GetInt32("User_ID");
                                        int userId = userIdNullable ?? 0;

                                        // Create a new cookie that expires in 30 days
                                        var cookieOptions = new CookieOptions
                                        {
                                            Expires = DateTime.Now.AddDays(30), // Set the expiration date to 30 days from now
                                            HttpOnly = true, // Make the cookie HTTP-only for security
                                            IsEssential = true, // Mark the cookie as essential
                                        };

                                        // Store the userId in the cookie
                                        if (remember)
                                        {
                                            HttpContext.Response.Cookies.Append("MyCookie", userId.ToString(), cookieOptions);
                                        }

                                        return RedirectToAction("Profile");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Fail");
                                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                                    }
                                }
                                else
                                {
                                    ModelState.AddModelError(string.Empty, "Your account is not active. Please contact the administrator.");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception occurred: " + ex.ToString());
                    ModelState.AddModelError(string.Empty, "An error occurred while processing your request.");
                }
            }

            return View(model);
        }


        public IActionResult edit_profile()
        {
            int? userIdNullable = HttpContext.Session.GetInt32("User_ID");
            int userId = userIdNullable ?? 0;

            // Check if the user is logged in and has a valid userId
            if (userId != 0)
            {

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT VolunteerID, FirstName, LastName, Email, MobileNumber, BirthDate, Gender, Country, PROV_CODE, TOWN_CODE, BRGY_CODE, YearStarted, CreatedDate, LastUpdateDate, IsNotify FROM T_Volunteer WHERE VolunteerID = @parsedVolunteerID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@parsedVolunteerID", userId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // The volunteer was found in the database
                                ViewBag.VolunteerID = reader["VolunteerID"];
                                ViewBag.FirstName = reader["FirstName"];
                                ViewBag.LastName = reader["LastName"];
                                ViewBag.Email = reader["Email"];
                                ViewBag.MobileNumber = reader["MobileNumber"];
                                ViewBag.BirthDate = reader["BirthDate"];
                                ViewBag.Gender = reader["Gender"];
                                ViewBag.Country = reader["Country"];
                                ViewBag.Province = reader["PROV_CODE"];
                                ViewBag.Town = reader["TOWN_CODE"];
                                ViewBag.Barangay = reader["BRGY_CODE"];
                                ViewBag.YearStarted = reader["YearStarted"];
                                ViewBag.CreatedDate = reader["CreatedDate"];
                                ViewBag.LastUpdateDate = reader["LastUpdateDate"];
                                ViewBag.IsNotify = reader["IsNotify"];
                            }
                        }
                    }
                }
            }

            return RedirectToAction("Profile");
        }
    }
}
