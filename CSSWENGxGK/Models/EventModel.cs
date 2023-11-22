using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSSWENGxGK.Models
{
    public class Event
    {
        public const int Ongoing = 0;
        public const int Finished = 1;
        public const int Canceled = 2;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventID { get; set; }

        public byte[] EventImage { get; set; }

        [Required(ErrorMessage = "Event Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Event Name should be between 2 and 100 characters.")]
        public string EventName { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public DateTime EventEndDate { get; set; }

        [Required]
        public string EventLocation { get; set; }

        [StringLength(280, ErrorMessage = "Event Description should be less than 280 characters.")]
        public string EventShortDesc { get; set; }

        public string EventLongDesc { get; set; }

        public int EventStatus { get; set; }
        // 0: "Ongoing"
        // 1: "Finished"
        // 2: "Canceled"
    }
}
