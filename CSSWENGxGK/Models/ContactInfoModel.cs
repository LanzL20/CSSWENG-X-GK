using System;
using System.ComponentModel.DataAnnotations;

namespace CSSWENGxGK.Models
{
    public class ContactInfo
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        public string Contact_Info { get; set; }
    }
}
