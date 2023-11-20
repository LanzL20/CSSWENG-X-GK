using System.ComponentModel.DataAnnotations;

namespace CSSWENGxGK.Models
{
    public class LogInfo
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please input one time password.")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Please input a valid one time password.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Please input a valid one time password.")]
        public string Password { get; set; }

        public bool IsRemember { get; set; }
    }
}
