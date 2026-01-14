using Microsoft.AspNetCore.Identity;

namespace Diversion.Models;

public class ModerationAction
{
    public int Id { get; set; }
    public string AdminId { get; set; }
    public IdentityUser Admin { get; set; }

    public string ActionType { get; set; }  // "BanUser", "DeleteEvent", "DeleteMessage", "WarnUser", "DismissReport"
    public string? TargetUserId { get; set; }
    public IdentityUser? TargetUser { get; set; }

    public string? TargetEntityType { get; set; }  // "Event", "CommunityMessage", "DirectMessage"
    public string? TargetEntityId { get; set; }

    public int? RelatedReportId { get; set; }  // Link to Report if action was from report review
    public Report? RelatedReport { get; set; }

    public string Reason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
