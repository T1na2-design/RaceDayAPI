using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs.Events
{
    public class EventUpdateRequest
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        public TimeSpan? EventTime { get; set; }

        public string Venue { get; set; }

        public string City { get; set; }

        public string Province { get; set; }

        public string EventType { get; set; }

        [Required]
        [Range(0.0, 10000.0)]
        public decimal Distance { get; set; }

        [Required]
        [RegularExpression("^(km|miles)$", ErrorMessage = "DistanceUnit must be 'km' or 'miles'.")]
        public string DistanceUnit { get; set; }

        public int? MaxParticipants { get; set; }

        public DateTime? RegistrationOpenDate { get; set; }

        public DateTime? RegistrationCloseDate { get; set; }

        public string Metadata { get; set; }
    }
}
