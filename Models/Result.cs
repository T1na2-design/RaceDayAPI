using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Result
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Enrolment))]
        public int EnrolmentId { get; set; }

        // Times recorded as TimeSpan representing hh:mm:ss.s
        public TimeSpan? FinishTime { get; set; }
        public TimeSpan? GunTime { get; set; }
        public TimeSpan? ChipTime { get; set; }

        public int? OverallPosition { get; set; }
        public int? GenderPosition { get; set; }
        public int? CategoryPosition { get; set; }

        // Pace could be stored as string like "4:30/km" or seconds-per-unit as decimal if preferred
        [StringLength(50)]
        public string Pace { get; set; }

        public bool IsDisqualified { get; set; }
        public string DisqualificationReason { get; set; }

        public string ResultNotes { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual Enrolment Enrolment { get; set; }
    }
}
