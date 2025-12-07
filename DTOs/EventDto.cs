using System.ComponentModel.DataAnnotations;

namespace Diversion.DTOs
{
    public class EventDto
    {
        public Guid Id { get; set; }
        public string? OrganizerId { get; set; }
        public string? OrganizerUsername { get; set; }
        public Guid InterestTagId { get; set; }
        public string? InterestTagName { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string? EventType { get; set; }
        public string? Location { get; set; }
        public string? MeetingUrl { get; set; }
        public bool RequiresRsvp { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateEventDto
    {
        [Required]
        public Guid InterestTagId { get; set; }

        [Required]
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        [Required]
        [RegularExpression("^(Online|InPerson)$", ErrorMessage = "EventType must be either 'Online' or 'InPerson'")]
        public string? EventType { get; set; }

        [StringLength(500)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? MeetingUrl { get; set; }

        public bool RequiresRsvp { get; set; } = false;
    }

    public class UpdateEventDto
    {
        public Guid? InterestTagId { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        [RegularExpression("^(Online|InPerson)$", ErrorMessage = "EventType must be either 'Online' or 'InPerson'")]
        public string? EventType { get; set; }

        [StringLength(500)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? MeetingUrl { get; set; }

        public bool? RequiresRsvp { get; set; }
    }

    public class EventDetailDto
    {
        public Guid Id { get; set; }
        public string? OrganizerId { get; set; }
        public string? OrganizerUsername { get; set; }
        public SubInterestDto? InterestTag { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string? EventType { get; set; }
        public string? Location { get; set; }
        public string? MeetingUrl { get; set; }
        public bool RequiresRsvp { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<EventAttendeeDto> Attendees { get; set; } = new();
        public int AttendeeCount { get; set; }
    }
}
