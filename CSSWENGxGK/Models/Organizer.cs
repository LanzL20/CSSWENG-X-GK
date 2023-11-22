using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSSWENGxGK.Models
{
    public class Organizer
    {
        [Key]
        public int OrganizerID { get; set; }

        [ForeignKey("Event")]
        public int EventID { get; set; }

        public string Name { get; set; }

        [RegularExpression(@"^(\+[0-9]{1,4}[\s-])?(?=[0-9\s-]*\b[0-9\s-]{7,20}\b)(?=[0-9\s-]*\b[0-9\s-]{7,20}\b)(\d+[\s-]?){7,20}$", ErrorMessage = "Invalid Mobile Number.")]
        [StringLength(20, ErrorMessage = "Mobile Number should be less than 20 characters.")]
        public string PhoneNumber { get; set;}

        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [StringLength(100, ErrorMessage = "Email should be less than 100 characters.")]
        public string Email { get; set; }
    }
}
