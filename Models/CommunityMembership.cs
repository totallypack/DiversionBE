using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Diversion.Models
{
    public class CommunityMembership
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CommunityId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "Member"; // "Owner", "Moderator", "Member"

        [Required]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Community Community { get; set; }
        public IdentityUser User { get; set; }
    }
}
