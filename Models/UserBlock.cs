using Microsoft.AspNetCore.Identity;

namespace Diversion.Models;

public class UserBlock
{
    public int Id { get; set; }
    public string BlockerId { get; set; }
    public IdentityUser Blocker { get; set; }

    public string BlockedUserId { get; set; }
    public IdentityUser BlockedUser { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? Reason { get; set; }  // Optional: why they blocked
}
