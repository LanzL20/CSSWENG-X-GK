using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Identity;

namespace CSSWENGxGK.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager)
        {
            _logger = logger;
            _userManager = userManager;

            CheckAndHandleCookie(); // Call the method in the constructor if needed
        }

        public async Task<IActionResult> Index()
        {
            CheckAndHandleCookie(); // Call the method at the beginning of the action

            // Your existing action logic goes here
            return View();
        }

        private void CheckAndHandleCookie()
        {
            HttpContext context = ControllerContext.HttpContext;

            if (context?.Request?.Cookies.TryGetValue("MyCookie", out string cookieValue) == true &&
                !string.IsNullOrEmpty(cookieValue) && int.TryParse(cookieValue, out int userId) &&
                context.Session != null)
            {
                // Store the user ID in the session
                context.Session.SetInt32("User_ID", userId);
                // You can perform additional initialization based on the cookie value if needed
            }
            else
            {
                // Handle the case where context or its properties are null, or where the cookie value is not valid
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
