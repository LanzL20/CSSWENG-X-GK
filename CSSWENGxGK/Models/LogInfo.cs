using System.ComponentModel.DataAnnotations;

namespace CSSWENGxGK.Models
{
    public class LogInfo
    {
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please input one time password.")]
        public string Password { get; set; }

        public bool IsRemember { get; set; }
    }
}
