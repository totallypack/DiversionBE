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
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? MeetingUrl { get; set; }
        public bool RequiresRsvp { get; set; }
        public decimal? TicketPrice { get; set; }
        public int? MaxAttendees { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RsvpStatus { get; set; }
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
        public string? StreetAddress { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(500)]
        public string? MeetingUrl { get; set; }

        public bool RequiresRsvp { get; set; } = false;

        public decimal? TicketPrice { get; set; }
        public int? MaxAttendees { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
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
        public string? StreetAddress { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(500)]
        public string? MeetingUrl { get; set; }

        public bool? RequiresRsvp { get; set; }

        public decimal? TicketPrice { get; set; }
        public int? MaxAttendees { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
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
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? MeetingUrl { get; set; }
        public bool RequiresRsvp { get; set; }
        public decimal? TicketPrice { get; set; }
        public int? MaxAttendees { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<EventAttendeeDto> Attendees { get; set; } = new();
        public int AttendeeCount { get; set; }
    }
}
