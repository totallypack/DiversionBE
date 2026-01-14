using Microsoft.AspNetCore.Identity;

namespace Diversion.Models;

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; }  // Recipient
    public IdentityUser User { get; set; }

    public string Type { get; set; }  // "FriendRequest", "EventRSVP", "NewMessage", "CommunityInvite", etc.
    public string? ReferenceId { get; set; }  // ID of related entity (requestId, eventId, messageId, etc.)
    public string Message { get; set; }  // "John Doe sent you a friend request"
    public string? ActionUrl { get; set; }  // "/friends/requests", "/events/123", etc.

    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
