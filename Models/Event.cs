using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Diversion.Models
{
    public class Event
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string OrganizerId { get; set; }
        [Required]
        public Guid InterestTagId { get; set; }
        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
        [Required]
        public DateTime StartDateTime { get; set; }
        [Required]
        public DateTime EndDateTime { get; set; }
        [Required]
        public string EventType { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? MeetingUrl { get; set; }

        public bool RequiresRsvp { get; set; }

        // Paid event fields
        public decimal? TicketPrice { get; set; }
        public int? MaxAttendees { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public IdentityUser Organizer { get; set; }
        public SubInterest InterestTag { get; set; }
        public ICollection<EventAttendee> Attendees { get; set; }

    }
}
