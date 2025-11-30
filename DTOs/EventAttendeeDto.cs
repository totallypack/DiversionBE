using System.ComponentModel.DataAnnotations;

namespace Diversion.DTOs
{
    public class EventAttendeeDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateEventAttendeeDto
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        [RegularExpression("^(Going|Interested|Not Going)$")]
        public string Status { get; set; } = "Interested";
    }

    public class UpdateEventAttendeeDto
    {
        [Required]
        [RegularExpression("^(Going|Interested|Not Going)$")]
        public string Status { get; set; }
    }
}
