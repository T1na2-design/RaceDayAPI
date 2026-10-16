using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [RegularExpression("^(Organiser|Participant)$", ErrorMessage = "Role must be 'Organiser' or 'Participant'.")]
        public string Role { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; }

        public string IdNumber { get; set; }

        public string EmergencyContactName { get; set; }

        [Phone]
        public string EmergencyContactNumber { get; set; }

        public string MedicalConditions { get; set; }

        public string ClubAffiliation { get; set; }
    }
}
