using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Enrolment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Participant))]
        public int ParticipantId { get; set; }

        [Required]
        [ForeignKey(nameof(Event))]
        public int EventId { get; set; }

        [Required]
        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }

        [StringLength(50)]
        public string Status { get; set; } // e.g., Registered, Cancelled, Pending

        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

        // Payment details, reference etc. can be added later

        // Navigation
        public virtual Participant Participant { get; set; }
        public virtual Event Event { get; set; }
        public virtual Category Category { get; set; }
        public virtual Result Result { get; set; }
    }
}
