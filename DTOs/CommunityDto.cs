using System.ComponentModel.DataAnnotations;

namespace Diversion.DTOs
{
    public class CommunityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CreatorId { get; set; } = string.Empty;
        public string CreatorUsername { get; set; } = string.Empty;
        public string? CreatorDisplayName { get; set; }
        public Guid? InterestId { get; set; }
        public string? InterestName { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPrivate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MemberCount { get; set; }
        public bool IsMember { get; set; }
        public string? UserRole { get; set; } // "Owner", "Moderator", "Member", or null if not a member
    }

    public class CreateCommunityDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public Guid? InterestId { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsPrivate { get; set; } = false;
    }

    public class UpdateCommunityDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsPrivate { get; set; }
    }

    public class CommunityMembershipDto
    {
        public Guid Id { get; set; }
        public Guid CommunityId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}
