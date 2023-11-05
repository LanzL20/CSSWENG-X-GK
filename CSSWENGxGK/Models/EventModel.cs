using System;
using System.ComponentModel.DataAnnotations;

namespace CSSWENGxGK.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        public int EventImage { get; set; }

        [Required(ErrorMessage = "Event Name is required.")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Event Name should be between 2 and 30 characters.")]
        public string EventName { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public string EventTime { get; set; }

        [Required]
        public string EventLocation { get; set; }

        public string EventShortDesc { get; set; }

        public string EventLongDesc { get; set; }
    }
}
