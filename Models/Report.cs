using Microsoft.AspNetCore.Identity;

namespace Diversion.Models;

public class Report
{
    public int Id { get; set; }
    public string ReporterId { get; set; }
    public IdentityUser Reporter { get; set; }

    public string ReportedEntityType { get; set; }  // "User", "Event", "CommunityMessage", "DirectMessage"
    public string ReportedEntityId { get; set; }  // ID of reported entity
    public string? ReportedUserId { get; set; }  // If reporting a user or user-generated content
    public IdentityUser? ReportedUser { get; set; }

    public string Reason { get; set; }  // "Harassment", "Spam", "Inappropriate Content", "Other"
    public string? Details { get; set; }  // Optional detailed description

    public string Status { get; set; }  // "Pending", "UnderReview", "Resolved", "Dismissed"
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByAdminId { get; set; }
    public IdentityUser? ReviewedByAdmin { get; set; }
    public string? AdminNotes { get; set; }
}
