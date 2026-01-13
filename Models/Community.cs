using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Diversion.Models
{
    public class Community
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        public string CreatorId { get; set; }

        public Guid? InterestId { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsPrivate { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public IdentityUser Creator { get; set; }
        public Interest? Interest { get; set; }
        public ICollection<CommunityMembership> Members { get; set; }
        public ICollection<CommunityMessage> Messages { get; set; }
    }
}
