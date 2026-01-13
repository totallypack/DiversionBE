using System.ComponentModel.DataAnnotations;

namespace Diversion.DTOs
{
    public class CareRelationshipDto
    {
        public Guid Id { get; set; }
        public string? CaregiverId { get; set; }
        public string? CaregiverUsername { get; set; }
        public string? CaregiverDisplayName { get; set; }
        public string? RecipientId { get; set; }
        public string? RecipientUsername { get; set; }
        public string? RecipientDisplayName { get; set; }
        public bool CanManageEvents { get; set; }
        public bool CanManageProfile { get; set; }
        public bool CanManageFriendships { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateCareRelationshipDto
    {
        [Required]
        public string? RecipientId { get; set; }

        public bool CanManageEvents { get; set; } = true;
        public bool CanManageProfile { get; set; } = true;
        public bool CanManageFriendships { get; set; } = true;
    }

    public class UpdateCareRelationshipPermissionsDto
    {
        public bool? CanManageEvents { get; set; }
        public bool? CanManageProfile { get; set; }
        public bool? CanManageFriendships { get; set; }
    }
}
