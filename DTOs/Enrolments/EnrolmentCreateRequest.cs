using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs.Enrolments
{
    public class EnrolmentCreateRequest
    {
        [Required]
        public int CategoryId { get; set; }
    }
}
