using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Identity;
namespace CSSWENGxGK.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<User> _userManager;

    public HomeController(ILogger<HomeController> logger, UserManager<User> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // Set ViewBag.IsAdmin to false by default
        ViewBag.IsAdmin = false;

        // Retrieve the UserId from the session
        if (HttpContext.Session.TryGetValue("UserId", out var userIdBytes) && userIdBytes.Length == sizeof(int))
        {
            int userId = BitConverter.ToInt32(userIdBytes);
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                // Update ViewBag.IsAdmin to true if the user has the "Administrator" role
                ViewBag.IsAdmin = userRoles.Contains("Administrator");
            }
        }

        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
