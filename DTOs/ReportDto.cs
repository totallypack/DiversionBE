namespace Diversion.DTOs;

public class CreateReportDto
{
    public string ReportedEntityType { get; set; }
    public string ReportedEntityId { get; set; }
    public string Reason { get; set; }
    public string? Details { get; set; }
}

public class ReportDto
{
    public int Id { get; set; }
    public string ReporterUsername { get; set; }
    public string ReportedEntityType { get; set; }
    public string ReportedEntityId { get; set; }
    public string? ReportedUsername { get; set; }
    public string Reason { get; set; }
    public string? Details { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByAdminUsername { get; set; }
    public string? AdminNotes { get; set; }
}

public class ReportReviewDto
{
    public string Status { get; set; }  // "Resolved" or "Dismissed"
    public string? AdminNotes { get; set; }
    public string? ActionType { get; set; }  // "BanUser", "DeleteContent", "WarnUser", null
    public string? ActionReason { get; set; }
}
