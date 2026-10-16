using System;

namespace RaceDay.DTOs.Enrolments
{
    public class EnrolmentResponse
    {
        public int Id { get; set; }
        public int ParticipantId { get; set; }
        public int EventId { get; set; }
        public int CategoryId { get; set; }
        public string Status { get; set; }
        public DateTime EnrolmentDate { get; set; }
    }
}
