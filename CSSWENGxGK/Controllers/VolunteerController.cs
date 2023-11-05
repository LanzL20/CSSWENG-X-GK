using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace CSSWENGxGK.Controllers
{

    public class VolunteerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;

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

        [HttpPost]
        public async Task<IActionResult> VerifyVolunteerID(string Vol_ID)
        {
            if (int.TryParse(Vol_ID, out int parsedVolunteerID) && parsedVolunteerID > 0)
            {
                // change to own conneciton string
                string connectionString = "Server=DESKTOP-SERVS0D;Database=cssweng;Trusted_Connection=True;TrustServerCertificate=True;";

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Volunteer model)
        {
            if (ModelState.IsValid)
            {
                Guid uniqueId = Guid.NewGuid();
                byte[] bytes = uniqueId.ToByteArray();

                int generatedID = BitConverter.ToInt32(bytes, 0);
                generatedID = Math.Abs(generatedID);

                string connectionString = "Server=DESKTOP-SERVS0D;Database=cssweng;Trusted_Connection=True;TrustServerCertificate=True;";

                // Define the SQL insert query
                string query = "SET IDENTITY_INSERT T_Volunteer ON;" +
                              "INSERT INTO T_Volunteer (VolunteerID, CreatedDate, LastUpdateDate, IsDeleted, IsActive, FirstName, LastName, Email, MobileNumber, BirthDate, Gender, Country, PROV_CODE, TOWN_CODE, BRGY_CODE, YearStarted) " +
                              "VALUES (@GeneratedID, @CreatedDate, @LastUpdateDate, @IsDeleted, @IsActive, @FirstName, @LastName, @Email, @MobileNumber, @BirthDate, @Gender, @Country, @PROV_CODE, @TOWN_CODE, @BRGY_CODE, @YearStarted);" +
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
                        command.Parameters.AddWithValue("@PROV_CODE", model.PROV_CODE);
                        command.Parameters.AddWithValue("@TOWN_CODE", model.TOWN_CODE);
                        command.Parameters.AddWithValue("@BRGY_CODE", model.BRGY_CODE);
                        command.Parameters.AddWithValue("@YearStarted", model.YearStarted);

                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Register");
            }

            return View(model);
        }


        // [HttpPost]
        // public IActionResult Login(Volunteer model)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         var user = _db.T_Volunteer
        //             .FirstOrDefault(v => v.Email == model.Email);

        //         if (user != null)
        //         {
        //             await HttpContext.SignInAsync(user, isPersistent: true); // This will create a persistent cookie
        //             return RedirectToAction("Dashboard", "Home");
        //         }
        //         else
        //         {
        //             ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        //         }
        //     }
        //     return View(model);
        // }
    }
}
