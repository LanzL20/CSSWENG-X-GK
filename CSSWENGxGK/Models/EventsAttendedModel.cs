using System;
using System.ComponentModel.DataAnnotations;

namespace CSSWENGxGK.Models
{
    public class EventsAttended
    {
        [Key]
        public int VolunteerID { get; set; }

        [Key]
        public int EventID { get; set; }

        [Required]
        public DateTime TimeIn { get; set; }

        [Required]
        public DateTime TimeOut { get; set; }
    }
}
