using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CSSWENGxGK.Data;
using CSSWENGxGK.Models;

public class UserController : Controller
{
    private UserManager<User> userManager;

    public UserController(UserManager<User> userManager)
    {
        this.userManager = userManager;
    }

    // put acutal field in database to determine if user or admin
    public IActionResult CheckUserRole(int userId)
    {
           
        var user = userManager.FindByIdAsync(userId).Result;

        if (user != null)
        {
            var isAdmin = userManager.IsInRoleAsync(user, "Admin").Result;

            if (isAdmin)
            {
                // The user is an admin
                // Handle admin-specific logic here
            }
            else
            {
                // The user is not an admin
                // Handle regular user-specific logic here
            }
        }
        else
        {
            // Handle the case where the user does not exist
        }

        return View();
    }
}
