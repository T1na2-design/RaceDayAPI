using System;
using System.Collections.Generic;

namespace RaceDay.DTOs.Events
{
    public class EventResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan? EventTime { get; set; }
        public string Venue { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string EventType { get; set; }
        public decimal Distance { get; set; }
        public string DistanceUnit { get; set; }
        public int? MaxParticipants { get; set; }
        public DateTime? RegistrationOpenDate { get; set; }
        public DateTime? RegistrationCloseDate { get; set; }
        public string Metadata { get; set; }
        public string Status { get; set; }
        public int OrganiserId { get; set; }

        public List<CategorySummary> Categories { get; set; } = new List<CategorySummary>();
    }

    public class CategorySummary
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public int EnrolmentCount { get; set; }
    }
}
