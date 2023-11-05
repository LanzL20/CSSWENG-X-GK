using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSSWENGxGK.Models
{
    public class ContactInfo
    {
        [ForeignKey("Event")]
        public int EventID { get; set; }

        [Required]
        public string Contact_Info { get; set; }
    }
}
