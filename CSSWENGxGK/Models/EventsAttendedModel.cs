using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSSWENGxGK.Models
{
    public class EventsAttended
    {
        [Key]
        [ForeignKey("Volunteer")]
        public int VolunteerID { get; set; }

        [ForeignKey("Event")]
        public int EventID { get; set; }

        [Required]
        public DateTime TimeIn { get; set; }

        [Required]
        public DateTime TimeOut { get; set; }
    }
}
