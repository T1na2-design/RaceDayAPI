using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs.Results
{
    public class ResultCreateRequest
    {
        [Required]
        public int EnrolmentId { get; set; }

        public TimeSpan? FinishTime { get; set; }
        public TimeSpan? GunTime { get; set; }
        public TimeSpan? ChipTime { get; set; }

        public int? OverallPosition { get; set; }
        public int? GenderPosition { get; set; }
        public int? CategoryPosition { get; set; }

        [StringLength(50)]
        public string Pace { get; set; }

        public bool IsDisqualified { get; set; }
        public string DisqualificationReason { get; set; }
        public string ResultNotes { get; set; }
    }
}
