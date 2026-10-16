using System;

namespace RaceDay.DTOs.Categories
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public decimal Distance { get; set; }
        public string DistanceUnit { get; set; }
        public int? AgeGroupMin { get; set; }
        public int? AgeGroupMax { get; set; }
        public string GenderRestriction { get; set; }
        public decimal EntryFee { get; set; }
        public decimal? LateEntryFee { get; set; }
        public int? MaxParticipants { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? CutOffTime { get; set; }
    }
}
