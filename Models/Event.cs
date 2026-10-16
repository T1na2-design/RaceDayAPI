using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Organiser))]
        public int OrganiserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        // Optional separate time field
        public TimeSpan? EventTime { get; set; }

        [StringLength(200)]
        public string Venue { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string Province { get; set; }

        [StringLength(100)]
        public string EventType { get; set; }

        // CRITICAL SCHEMA FIX: Distance and DistanceUnit
        [Column(TypeName = "decimal(8,2)")]
        public decimal Distance { get; set; }

        [Required]
        [StringLength(10)]
        public string DistanceUnit { get; set; } // e.g., "km" or "miles"

        public int? MaxParticipants { get; set; }

        public DateTime? RegistrationOpenDate { get; set; }

        public DateTime? RegistrationCloseDate { get; set; }

        public string Metadata { get; set; }

        // Status (e.g., Draft, Published, Cancelled)
        [StringLength(50)]
        public string Status { get; set; }

        // Navigation
        public virtual Organiser Organiser { get; set; }
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
        public virtual ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
