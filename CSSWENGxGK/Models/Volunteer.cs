using System;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSSWENGxGK.Models
{
    public class Volunteer : IdentityUser
    {
        [Key]
        public int VolunteerID { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "First Name should be between 2 and 30 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Last Name should be between 2 and 30 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [StringLength(100, ErrorMessage = "Email should be less than 100 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile Number is required.")]
        [RegularExpression(@"^(\+[0-9]{1,4}[\s-])?(?=[0-9\s-]*\b[0-9\s-]{7,20}\b)(?=[0-9\s-]*\b[0-9\s-]{7,20}\b)(\d+[\s-]?){7,20}$", ErrorMessage = "Invalid Mobile Number.")]
        [StringLength(20, ErrorMessage = "Mobile Number should be less than 20 characters.")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Birth Date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid Birth Date.")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [StringLength(10, ErrorMessage = "Gender should be less than 10 characters.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(30, ErrorMessage = "Country should be less than 30 characters.")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Province Code is required.")]
        [StringLength(10, ErrorMessage = "Province Code should be less than 10 characters.")]
        public string PROV_CODE { get; set; }

        [Required(ErrorMessage = "Town Code is required.")]
        [StringLength(10, ErrorMessage = "Town Code should be less than 10 characters.")]
        public string TOWN_CODE { get; set; }

        [Required(ErrorMessage = "Barangay Code is required.")]
        [StringLength(10, ErrorMessage = "Barangay Code should be less than 10 characters.")]
        public string BRGY_CODE { get; set; }

        [Required(ErrorMessage = "Year Started is required.")]
        [Range(1900, 2100, ErrorMessage = "Year Started should be between 1900 and 2100.")]
        public int YearStarted { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime LastUpdateDate { get; set; }

        [Required]
        public bool IsDeleted { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
