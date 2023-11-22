using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSSWENGxGK.Models
{
    public class ContactInfo
    {
        [Key]
        [ForeignKey("Event")]
        public int EventID { get; set; }

        [Required]
        public string Contact_Name { get; set; }

        public string Contact_Number { get; set; }

        public string Contact_Email { get; set; }
    }
}
