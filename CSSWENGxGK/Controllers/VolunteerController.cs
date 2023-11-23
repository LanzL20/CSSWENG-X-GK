using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Data.SqlClient;
using BCrypt.Net;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.AspNetCore.Http;
using ZXing;
using ZXing.Windows.Compatibility;

namespace CSSWENGxGK.Controllers
{

    public class VolunteerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        Emailer emailer = new Emailer();
        string connectionString = "Server=localhost\\SQLEXPRESS;Database=cssweng;Trusted_Connection=True;TrustServerCertificate=True;";

        public VolunteerController(ApplicationDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Login()
        {
            return View();
        }

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

                    string query = "SELECT Password, VolunteerID, IsActive, OtpUsed, LastOtpTime FROM T_Volunteer WHERE Email = @Email";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool isActive = reader.GetBoolean(reader.GetOrdinal("IsActive"));
                                bool otpUsed = reader.GetBoolean(reader.GetOrdinal("OtpUsed"));
                                DateTime lastOtpTime = reader.GetDateTime(reader.GetOrdinal("LastOtpTime"));

                                if (isActive)
                                {
                                    // Check if the OTP was generated more than 5 minutes ago
                                    if ((DateTime.Now - lastOtpTime).TotalMinutes > 5)
                                    {
                                        ModelState.AddModelError(string.Empty, "The OTP has expired. Please generate a new one.");
                                    }
                                    else if (otpUsed)
                                    {
                                        ModelState.AddModelError(string.Empty, "The OTP has already been used. Please generate a new one.");
                                    }
                                    else
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

                                            MarkOtpAsUsed(userId);
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
                                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                                        }
                                    }
                                }
                                else
                                {
                                    ModelState.AddModelError(string.Empty, "Your account is not active. Please contact the administrator.");
                                    return RedirectToAction("Reactivate");
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

        public IActionResult Register()
        {
            return View();
        }

        //add verification like duplciate emails and other things
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Volunteer model)
        {
            model.Password = GenerateRandomPassword();

            if (ModelState.IsValid)
            {
                if (EmailExists(model.Email))
                {
                    ViewBag.EmailErrorMessage = "Email Already Exists";
                    return View("Register", model);
                }

                bool real_email = await emailer.Send_Welcome(model.Email);

                if (!real_email)
                {
                    return View("Register", model);
                }

                string query2 = "SELECT COUNT(*) FROM T_Volunteer";

                // Define the SQL insert query
                string query = "SET IDENTITY_INSERT T_Volunteer ON;" +
                              "INSERT INTO T_Volunteer (VolunteerID, CreatedDate, LastUpdateDate, IsDeleted, IsActive, IsNotify, Password, FirstName, LastName, Email, MobileNumber, BirthDate, Gender, Country, PROV_CODE, TOWN_CODE, BRGY_CODE, YearStarted, LastOtpTime, OtpUsed) " +
                              "VALUES (@GeneratedID, @CreatedDate, @LastUpdateDate, @IsDeleted, @IsActive, @IsNotify, @Password, @FirstName, @LastName, @Email, @MobileNumber, @BirthDate, @Gender, @Country, @PROV_CODE, @TOWN_CODE, @BRGY_CODE, @YearStarted, @LastOtpTime, @OtpUsed);" +
                              "SET IDENTITY_INSERT T_Volunteer OFF;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    int generatedID = 0;

                    // Use current timestamp to ensure uniqueness
                    long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    // Generate a random 3-digit number
                    int randomPart = new Random().Next(100, 999);

                    // Combine timestamp and random number
                    long combinedValue = long.Parse($"{timestamp}{randomPart}");

                    // Take the last 9 digits as the unique ID
                    generatedID = (int)(combinedValue % 1_000_000_000);

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

                        // Set LastOtpTime to now and OtpUsed to true
                        command.Parameters.AddWithValue("@LastOtpTime", DateTime.Now);
                        command.Parameters.AddWithValue("@OtpUsed", true);

                        command.ExecuteNonQuery();

                        var new_user = new User
                        {

                        };

                        new_user.UserName = generatedID.ToString();
                        new_user.Email = model.Email;

                        //create user and add role
                        await _userManager.CreateAsync(new_user, "Password123!");
                        await _userManager.AddToRoleAsync(new_user, "User");

                        var roles = await _userManager.GetRolesAsync(new_user);

                        return RedirectToAction("Login");
                    }
                }
            }
            else
            {
                foreach (var entry in ModelState)
                {
                    if (entry.Value.Errors.Any())
                    {
                        Console.WriteLine($"Error in {entry.Key}: {entry.Value.Errors.First().ErrorMessage}");
                    }
                }

                return View("Register", model);
            }
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var randomPassword = new string(Enumerable.Repeat(chars, 12) // Adjust the length as needed
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return randomPassword;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateOtp(string email)
        {
            if (!EmailExists(email))
            {
                ViewBag.EmailErrorMessage = "Account Does Not Exist";
                ModelState.AddModelError(string.Empty, "User not found.");
                return View("Login");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Step 1: Retrieve VolunteerID based on the email
                    string selectQuery = "SELECT VolunteerID FROM T_Volunteer WHERE Email = @Email";

                    using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@Email", email);

                        using (SqlDataReader reader = selectCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int volunteerId = reader.GetInt32(0);

                                reader.Close();

                                // Step 2: Update user information with OTP
                                string updateQuery = "UPDATE T_Volunteer SET Password = @Otp, OtpUsed = 0, LastOtpTime = @CurrentTime WHERE VolunteerID = @VolunteerID";

                                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                                {
                                    // Generate and send OTP
                                    string OTP = await emailer.Send_OTP(email);

                                    // Hash the OTP using BCrypt
                                    string hashedOTP = BCrypt.Net.BCrypt.HashPassword(OTP);

                                    // Set parameters for the update command
                                    updateCommand.Parameters.AddWithValue("@Otp", hashedOTP); // Fix: Use consistent parameter name
                                    updateCommand.Parameters.AddWithValue("@CurrentTime", DateTime.Now);
                                    updateCommand.Parameters.AddWithValue("@VolunteerID", volunteerId);

                                    // Execute the update command
                                    updateCommand.ExecuteNonQuery();
                                }

                            }
                            else
                            {
                                // Handle the case when no user is found with the provided email
                                ViewBag.EmailErrorMessage = "Account Does Not Exist";
                                ModelState.AddModelError(string.Empty, "User not found.");
                                return View("Login");
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
            return Ok(new { message = "OTP generated successfully" });
        }

        private void MarkOtpAsUsed(int volunteerId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string updateQuery = "UPDATE T_Volunteer SET OtpUsed = 1, LastOtpTime = @CurrentTime WHERE VolunteerID = @VolunteerID";

                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@CurrentTime", DateTime.Now);
                    updateCommand.Parameters.AddWithValue("@VolunteerID", volunteerId);

                    updateCommand.ExecuteNonQuery();
                }
            }
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

                // Draw the Volunteer ID text
                using (Font font = new Font(FontFamily.GenericSansSerif, 12))
                {
                    string volunteerIdText = $"Volunteer ID: {text}";

                    // Calculate the position to center the text
                    float x = (qrCodeBitmap.Width - g.MeasureString(volunteerIdText, font).Width) / 2;
                    float y = qrCodeBitmap.Height - 30;

                    g.DrawString(volunteerIdText, font, Brushes.Black, new PointF(x, y));
                }
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

                                    string updateQuery = "UPDATE T_Volunteer SET FirstName = @FirstName, LastName = @LastName, MobileNumber = @MobileNumber, BirthDate = @BirthDate, Gender = @Gender, Country = @Country, PROV_CODE = @PROV_CODE, TOWN_CODE = @TOWN_CODE, BRGY_CODE = @BRGY_CODE, YearStarted = @YearStarted, IsNotify = @IsNotify WHERE VolunteerID = @VolunteerID";

                                    using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                                    {
                                        updateCommand.Parameters.AddWithValue("@FirstName", model.FirstName);
                                        updateCommand.Parameters.AddWithValue("@LastName", model.LastName);
                                        updateCommand.Parameters.AddWithValue("@MobileNumber", model.MobileNumber);
                                        updateCommand.Parameters.AddWithValue("@BirthDate", model.BirthDate);
                                        updateCommand.Parameters.AddWithValue("@Gender", model.Gender);
                                        updateCommand.Parameters.AddWithValue("@Country", model.Country);
                                        updateCommand.Parameters.AddWithValue("@PROV_CODE", model.PROV_CODE);
                                        updateCommand.Parameters.AddWithValue("@TOWN_CODE", model.TOWN_CODE);
                                        updateCommand.Parameters.AddWithValue("@BRGY_CODE", model.BRGY_CODE);
                                        updateCommand.Parameters.AddWithValue("@YearStarted", model.YearStarted);
                                        updateCommand.Parameters.AddWithValue("@IsNotify", model.IsNotify);
                                        updateCommand.Parameters.AddWithValue("@VolunteerID", userId);

                                        try
                                        {
                                            int rowsAffected = updateCommand.ExecuteNonQuery();

                                            if (rowsAffected > 0)
                                            {
                                                // The update was successful, and rows were modified
                                                Console.WriteLine("ACCEPTED");
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

        public IActionResult Sign_Out()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("MyCookie");
            return RedirectToAction("Login");
        }

        public IActionResult Reactivate()
        {
            return View();
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

                        string query = "SELECT VolunteerID, FirstName, LastName, IsActive FROM T_Volunteer WHERE VolunteerID = @parsedVolunteerID";

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
                                    var Active = reader["IsActive"];


                                    if (!(bool)Active)
                                    {
                                        // This block is executed if 'Active' is false.
                                        // It returns a JSON object indicating an error condition.
                                        return Json(new Successful_Volunteer
                                        {
                                            found = false,
                                            volunteerID = 0,
                                            firstName = "Volunteer",
                                            lastName = "Not Found"
                                        });
                                    }


                                    Successful_Volunteer newVolunteer = new Successful_Volunteer
                                    {
                                        found = true,
                                        volunteerID = (int)volunteerID,
                                        firstName = (string)firstName,
                                        lastName = (string)lastName
                                    };


                                    reader.Close();

                                    int selectedEvent = HttpContext.Session.GetInt32("Selected_event") ?? -1;

                                    // Insert the record into the T_EventsAttended table
                                    string insertQuery = "INSERT INTO T_EventsAttended (VolunteerID, EventID) " + "VALUES (@VolunteerID, @EventID)";

                                    try
                                    {
                                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                                        {
                                            insertCommand.Parameters.AddWithValue("@VolunteerID", parsedVolunteerID);
                                            insertCommand.Parameters.AddWithValue("@EventID", selectedEvent);

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

        [HttpPost]
        public async Task<IActionResult> Reactivate(string Email, string Password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string email = Email;
                    string password = Password;

                    string query = "SELECT Password, VolunteerID, IsActive, OtpUsed, LastOtpTime FROM T_Volunteer WHERE Email = @Email";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool isActive = reader.GetBoolean(reader.GetOrdinal("IsActive"));
                                bool otpUsed = reader.GetBoolean(reader.GetOrdinal("OtpUsed"));
                                DateTime lastOtpTime = reader.GetDateTime(reader.GetOrdinal("LastOtpTime"));
                                int volunteerId = reader.GetInt32(reader.GetOrdinal("VolunteerID"));
                                string hashedPassword = reader["Password"].ToString(); // Retrieve hashedPassword before closing the reader

                                // Close the reader
                                reader.Close();

                                if (!isActive)
                                {
                                    // Check if the OTP was generated more than 5 minutes ago
                                    if ((DateTime.Now - lastOtpTime).TotalMinutes > 5)
                                    {
                                        ModelState.AddModelError(string.Empty, "The OTP has expired. Please generate a new one.");
                                    }
                                    else if (otpUsed)
                                    {
                                        ModelState.AddModelError(string.Empty, "The OTP has already been used. Please generate a new one.");
                                    }
                                    else
                                    {
                                        if (BCrypt.Net.BCrypt.Verify(password, hashedPassword))
                                        {
                                            // Update IsActive to true and set LastUpdateDate to current time
                                            string updateQuery = "UPDATE T_Volunteer SET IsActive = 1, LastUpdateDate = GETDATE() WHERE Email = @Email";
                                            using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                                            {
                                                updateCommand.Parameters.AddWithValue("@Email", email);
                                                updateCommand.ExecuteNonQuery();
                                            }

                                            // Mark OTP as used
                                            MarkOtpAsUsed(volunteerId);

                                            return RedirectToAction("Login");
                                        }
                                        else
                                        {
                                            ModelState.AddModelError(string.Empty, "Invalid Reactivate attempt.");
                                        }
                                    }
                                }
                                else
                                {
                                    ModelState.AddModelError(string.Empty, "Your account is already active.");
                                    return RedirectToAction("Login");
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

            return RedirectToAction("Reactivate");
        }

        public async Task<IActionResult> Renew()
        {
            int? userIdNullable = HttpContext.Session.GetInt32("User_ID");
            int userId = userIdNullable ?? 0;

            if (userId != 0)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string email = string.Empty;
                    string emailQuery = "SELECT Email FROM T_Volunteer WHERE VolunteerID = @parsedVolunteerID";
                    using (SqlCommand emailCommand = new SqlCommand(emailQuery, connection))
                    {
                        emailCommand.Parameters.AddWithValue("@parsedVolunteerID", userId);

                        using (SqlDataReader emailReader = emailCommand.ExecuteReader())
                        {
                            if (emailReader.Read())
                            {
                                email = emailReader["Email"].ToString();
                            }
                            else
                            {
                                // Log the reason for redirection
                                Console.WriteLine("VolunteerID not found. Redirecting to Login.");
                                return RedirectToAction("Login");
                            }
                        }
                    }

                    // Step 2: Perform the update
                    string updateQuery = "UPDATE T_Volunteer SET LastUpdateDate = GETDATE() WHERE VolunteerID = @parsedVolunteerID";
                    using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@parsedVolunteerID", userId);
                        updateCommand.ExecuteNonQuery();
                    }

                    Console.WriteLine("Update successful. Redirecting to Profile.");
                    return RedirectToAction("Profile");
                }
            }
            else
            {
                // Log the case where userId is 0
                Console.WriteLine("Invalid userId. Redirecting to Login.");
                return RedirectToAction("Login");
            }
        }


    }
}
