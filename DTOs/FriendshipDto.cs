using System.ComponentModel.DataAnnotations;

namespace Diversion.DTOs
{
    public class FriendshipDto
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public string? FriendId { get; set; }
        public string? FriendUsername { get; set; }
        public string? FriendDisplayName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateFriendshipDto
    {
        [Required]
        public string? FriendId { get; set; }
    }

    public class UserSearchDto
    {
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? DisplayName { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public bool IsFriend { get; set; }
    }
}
