using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs.Categories
{
    public class CategoryCreateRequest
    {
        [Required]
        public string CategoryName { get; set; }

        public string Description { get; set; }

        [Range(0.0, 10000.0)]
        public decimal Distance { get; set; }

        [RegularExpression("^(km|miles)$", ErrorMessage = "DistanceUnit must be 'km' or 'miles'.")]
        public string DistanceUnit { get; set; }

        public int? AgeGroupMin { get; set; }
        public int? AgeGroupMax { get; set; }

        [StringLength(20)]
        public string GenderRestriction { get; set; }

        [Required]
        [Range(0.0, 1000000.0)]
        public decimal EntryFee { get; set; }

        [Range(0.0, 1000000.0)]
        public decimal? LateEntryFee { get; set; }

        public int? MaxParticipants { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? CutOffTime { get; set; }
    }
}
