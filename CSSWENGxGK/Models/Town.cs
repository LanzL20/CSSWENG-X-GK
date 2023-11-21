using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSSWENGxGK.Models
{
    public class Town
    {
        [ForeignKey("Province")]
        [Required]
        [StringLength(10), MinLength(10)]
        public string Prov_Code { get; set; }

        [Key]
        [Required]
        [StringLength(10), MinLength(10)]
        public string Town_Code { get; set; }

        [Required]
        [StringLength(50)]
        public string Town_Name { get; set; }
    }
}