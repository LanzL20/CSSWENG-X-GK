using System.ComponentModel.DataAnnotations;

namespace CSSWENGxGK.Models
{
    public class Provice
    {
        [Key]
        [Required]
        [StringLength(10), MinLength(10)]
        public string Prov_Code { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Prov_Name { get; set; }
    }
}