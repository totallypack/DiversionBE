using System.ComponentModel.DataAnnotations;

namespace Diversion.DTOs
{
    public class CaregiverRequestDto
    {
        public Guid Id { get; set; }
        public string? SenderId { get; set; }
        public string? SenderUsername { get; set; }
        public string? SenderDisplayName { get; set; }
        public string? RecipientId { get; set; }
        public string? RecipientUsername { get; set; }
        public string? RecipientDisplayName { get; set; }
        public string Status { get; set; } = "Pending";
        public string? RequestMessage { get; set; }
        public bool RequestCanManageEvents { get; set; }
        public bool RequestCanManageProfile { get; set; }
        public bool RequestCanManageFriendships { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }

    public class SendCaregiverRequestDto
    {
        [Required]
        public string? RecipientId { get; set; }

        [MaxLength(500)]
        public string? RequestMessage { get; set; }

        public bool RequestCanManageEvents { get; set; } = true;
        public bool RequestCanManageProfile { get; set; } = true;
        public bool RequestCanManageFriendships { get; set; } = true;
    }

    public class AcceptCaregiverRequestDto
    {
        // Recipient can override requested permissions when accepting
        public bool? CanManageEvents { get; set; }
        public bool? CanManageProfile { get; set; }
        public bool? CanManageFriendships { get; set; }
    }
}
