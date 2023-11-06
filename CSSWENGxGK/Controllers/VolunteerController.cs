using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Data.SqlClient;
using BCrypt.Net;

namespace CSSWENGxGK.Controllers
{

    public class VolunteerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
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

        public IActionResult Register()
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
                if(EmailExists(model.Email))
                {
                    return RedirectToAction("Register");
                }
                
            
                Guid uniqueId = Guid.NewGuid();
                byte[] bytes = uniqueId.ToByteArray();

                int generatedID = BitConverter.ToInt32(bytes, 0);
                generatedID = Math.Abs(generatedID);

                // Define the SQL insert query
                string query = "SET IDENTITY_INSERT T_Volunteer ON;" +
                              "INSERT INTO T_Volunteer (VolunteerID, CreatedDate, LastUpdateDate, IsDeleted, IsActive, IsNotify, Password, FirstName, LastName, Email, MobileNumber, BirthDate, Gender, Country, PROV_CODE, TOWN_CODE, BRGY_CODE, YearStarted) " +
                              "VALUES (@GeneratedID, @CreatedDate, @LastUpdateDate, @IsDeleted, @IsActive, @IsNotify, @Password, @FirstName, @LastName, @Email, @MobileNumber, @BirthDate, @Gender, @Country, @PROV_CODE, @TOWN_CODE, @BRGY_CODE, @YearStarted);" +
                              "SET IDENTITY_INSERT T_Volunteer OFF;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
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

                        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password,16);
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
                    }
                }

                return RedirectToAction("Register");
            }

            return View(model);
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

                    string query = "SELECT Password FROM T_Volunteer WHERE Email = email";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string hashedPassword = reader["Password"].ToString();
                                if (BCrypt.Net.BCrypt.Verify(password, hashedPassword))
                                {
                                    // Passwords match; user can be authenticated
                                    Console.WriteLine("Success");

                                    if (reader["VolunteerID"] is int volunteerID)
                                    {
                                        HttpContext.Session.SetInt32("User_ID", volunteerID);
                                    }

                                    // Retrieve the User_ID from the session with a default value of 0 if it's null this is for debugging
                                    int? userIdNullable = HttpContext.Session.GetInt32("User_ID");
                                    int userId = userIdNullable ?? 0;
                                    
                                    //await HttpContext.SignInAsync(user, isPersistent: true);
                                    return RedirectToAction("Dashboard", "Home");
                                }
                                else
                                {
                                    Console.WriteLine("Fail");
                                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
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

    }
}
