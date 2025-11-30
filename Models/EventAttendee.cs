using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Diversion.Models
{
    public class EventAttendee
    {
        [Key]
        public Guid Id { get; set; }
        [Required] 
        public Guid EventId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Event Event { get; set; }
        public IdentityUser User { get; set; }
    }
}
