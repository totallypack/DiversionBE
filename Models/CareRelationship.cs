using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Diversion.Models
{
    public class CareRelationship
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string CaregiverId { get; set; }

        [Required]
        public string RecipientId { get; set; }

        // Granular permission flags
        public bool CanManageEvents { get; set; } = true;
        public bool CanManageProfile { get; set; } = true;
        public bool CanManageFriendships { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public IdentityUser Caregiver { get; set; }
        public IdentityUser Recipient { get; set; }
    }
}
