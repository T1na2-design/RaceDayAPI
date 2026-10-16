using System;

namespace RaceDay.DTOs.Auth
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string IdNumber { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyContactNumber { get; set; }
        public string MedicalConditions { get; set; }
        public string ClubAffiliation { get; set; }
    }
}
