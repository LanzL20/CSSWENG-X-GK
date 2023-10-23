using System.ComponentModel.DataAnnotations;

namespace CSSWENGxGK.Models;
public class Volunteer
{
    [Key]
    public int VolunteerID { get; set; }

    [Required]
    [StringLength(30)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(30)]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; }

    [Required]
    [StringLength(100)]
    public string MobileNumber { get; set; }

    [Required]
    public DateTime BirthDate { get; set; }

    [Required]
    [StringLength(10)]
    public string Gender { get; set; }

    [Required]
    [StringLength(10)]
    public string Country { get; set; }

    [Required]
    [StringLength(10)]
    public string PROV_CODE { get; set; }

    [Required]
    [StringLength(10)]
    public string TOWN_CODE { get; set; }

    [Required]
    [StringLength(10)]
    public string BRGY_CODE { get; set; }

    [Required]
    public DateTime YearStarted { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    [Required]
    public DateTime LastUpdateDate { get; set; }

    [Required]
    public bool IsDeleted { get; set; }

    [Required]
    public bool IsActive { get; set; }
}