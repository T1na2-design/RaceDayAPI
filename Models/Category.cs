using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Event))]
        public int EventId { get; set; }

        [Required]
        [StringLength(200)]
        public string CategoryName { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal Distance { get; set; }

        [StringLength(10)]
        public string DistanceUnit { get; set; }

        public int? AgeGroupMin { get; set; }
        public int? AgeGroupMax { get; set; }

        [StringLength(20)]
        public string GenderRestriction { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal EntryFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? LateEntryFee { get; set; }

        public int? MaxParticipants { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? CutOffTime { get; set; }

        // Navigation
        public virtual Event Event { get; set; }
        public virtual ICollection<Enrolment> Enrolments { get; set; }
    }
}
