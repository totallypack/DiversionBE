using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Diversion.Models
{
    public class CaregiverRequest
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string SenderId { get; set; }  // Caregiver sending request

        [Required]
        public string RecipientId { get; set; }  // Care recipient

        public CaregiverRequestStatus Status { get; set; } = CaregiverRequestStatus.Pending;

        // Optional message explaining the request
        [MaxLength(500)]
        public string? RequestMessage { get; set; }

        // Requested permissions (can be adjusted during acceptance)
        public bool RequestCanManageEvents { get; set; } = true;
        public bool RequestCanManageProfile { get; set; } = true;
        public bool RequestCanManageFriendships { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }

        // Navigation properties
        public IdentityUser Sender { get; set; }
        public IdentityUser Recipient { get; set; }
    }
}
