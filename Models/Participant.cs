using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Participant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        // Navigation
        public virtual User User { get; set; }

        public virtual ICollection<Enrolment> Enrolments { get; set; }
        public virtual ICollection<Result> Results { get; set; }
    }
}
