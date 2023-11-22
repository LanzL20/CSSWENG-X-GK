using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSSWENGxGK.Models
{
    public class EventsAttended
    {
        [Key] // Specify VolunteerID as the primary key
        [ForeignKey("Volunteer")]
        public int VolunteerID { get; set; }

        [Key] // Specify EventID as the primary key
        [ForeignKey("Event")]
        public int EventID { get; set; }
    }
}
