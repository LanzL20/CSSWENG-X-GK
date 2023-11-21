using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSSWENGxGK.Models
{
    public class Barangay
    {
        [ForeignKey("Town")]
        [Required]
        [StringLength(10), MinLength(10)]
        public string Town_Code { get; set; }

        [Key]
        [Required]
        [StringLength(10), MinLength(10)]
        public string Barangay_Code { get; set; }

        [Required]
        [StringLength(50)]
        public string Brgy_Name { get; set; }
    }
}