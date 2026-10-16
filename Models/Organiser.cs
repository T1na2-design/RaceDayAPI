using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Organiser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        // Additional organiser-specific fields can be added here (e.g., organisation name)
        [StringLength(200)]
        public string OrganisationName { get; set; }

        // Navigation
        public virtual User User { get; set; }
        public virtual ICollection<Event> Events { get; set; }
    }
}
