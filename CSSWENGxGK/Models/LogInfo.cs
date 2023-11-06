using System.ComponentModel.DataAnnotations;

namespace CSSWENGxGK.Models
{
    public class LogInfo
    {
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        public bool IsRemember { get; set; }
    }
}
