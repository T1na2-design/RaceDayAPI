using System;

namespace RaceDay.DTOs.Results
{
    public class ResultResponse
    {
        public int Id { get; set; }
        public int EnrolmentId { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public TimeSpan? GunTime { get; set; }
        public TimeSpan? ChipTime { get; set; }
        public int? OverallPosition { get; set; }
        public int? GenderPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public string Pace { get; set; }
        public bool IsDisqualified { get; set; }
        public string DisqualificationReason { get; set; }
        public string ResultNotes { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
