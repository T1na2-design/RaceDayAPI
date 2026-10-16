using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Phone]
        [StringLength(30)]
        public string PhoneNumber { get; set; }

        // Role: "Organiser" or "Participant"
        [Required]
        [StringLength(50)]
        public string Role { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(50)]
        public string Gender { get; set; }

        [StringLength(50)]
        public string IdNumber { get; set; }

        [StringLength(200)]
        public string EmergencyContactName { get; set; }

        [Phone]
        [StringLength(30)]
        public string EmergencyContactNumber { get; set; }

        public string MedicalConditions { get; set; }

        [StringLength(200)]
        public string ClubAffiliation { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Organiser Organiser { get; set; }
        public virtual Participant Participant { get; set; }
    }
}
