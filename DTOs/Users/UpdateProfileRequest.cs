using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs.Users
{
    public class UpdateProfileRequest
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
