using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Diversion.DTOs;
using Diversion.Models;
using Diversion.Constants;

namespace Diversion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        [HttpPost]
        public async Task<ActionResult<ReportDto>> CreateReport([FromBody] CreateReportDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Basic validation
            if (string.IsNullOrWhiteSpace(dto.ReportedEntityType) ||
                string.IsNullOrWhiteSpace(dto.ReportedEntityId) ||
                string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest("ReportedEntityType, ReportedEntityId, and Reason are required");

            // Determine ReportedUserId based on entity type
            string? reportedUserId = null;

            switch (dto.ReportedEntityType)
            {
                case "User":
                    reportedUserId = dto.ReportedEntityId;
                    var reportedUser = await _context.Users.FindAsync(reportedUserId);
                    if (reportedUser == null)
                        return BadRequest("Reported user not found");

                    if (reportedUserId == userId)
                        return BadRequest("Cannot report yourself");
                    break;

                case "Event":
                    var evt = await _context.Events.FindAsync(Guid.Parse(dto.ReportedEntityId));
                    if (evt == null)
                        return BadRequest("Reported event not found");
                    reportedUserId = evt.OrganizerId;
                    break;

                case "CommunityMessage":
                    var communityMsg = await _context.CommunityMessages.FindAsync(int.Parse(dto.ReportedEntityId));
                    if (communityMsg == null)
                        return BadRequest("Reported community message not found");
                    reportedUserId = communityMsg.SenderId;
                    break;

                case "DirectMessage":
                    var directMsg = await _context.DirectMessages.FindAsync(Guid.Parse(dto.ReportedEntityId));
                    if (directMsg == null)
                        return BadRequest("Reported direct message not found");
                    reportedUserId = directMsg.SenderId;
                    break;

                default:
                    return BadRequest("Invalid ReportedEntityType. Must be: User, Event, CommunityMessage, or DirectMessage");
            }

            var report = new Report
            {
                ReporterId = userId,
                ReportedEntityType = dto.ReportedEntityType,
                ReportedEntityId = dto.ReportedEntityId,
                ReportedUserId = reportedUserId,
                Reason = dto.Reason,
                Details = dto.Details,
                Status = ReportStatusConstants.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            var result = await _context.Reports
                .AsNoTracking()
                .Where(r => r.Id == report.Id)
                .Select(r => new ReportDto
                {
                    Id = r.Id,
                    ReporterUsername = r.Reporter.UserName ?? "",
                    ReportedEntityType = r.ReportedEntityType,
                    ReportedEntityId = r.ReportedEntityId,
                    ReportedUsername = r.ReportedUser != null ? r.ReportedUser.UserName : null,
                    Reason = r.Reason,
                    Details = r.Details,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    ReviewedAt = r.ReviewedAt,
                    ReviewedByAdminUsername = r.ReviewedByAdmin != null ? r.ReviewedByAdmin.UserName : null,
                    AdminNotes = r.AdminNotes
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetMyReports), result);
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<ReportDto>>> GetMyReports()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var reports = await _context.Reports
                .AsNoTracking()
                .Where(r => r.ReporterId == userId)
                .Select(r => new ReportDto
                {
                    Id = r.Id,
                    ReporterUsername = r.Reporter.UserName ?? "",
                    ReportedEntityType = r.ReportedEntityType,
                    ReportedEntityId = r.ReportedEntityId,
                    ReportedUsername = r.ReportedUser != null ? r.ReportedUser.UserName : null,
                    Reason = r.Reason,
                    Details = r.Details,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    ReviewedAt = r.ReviewedAt,
                    ReviewedByAdminUsername = r.ReviewedByAdmin != null ? r.ReviewedByAdmin.UserName : null,
                    AdminNotes = r.AdminNotes
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(reports);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReportDto>> GetReport(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var report = await _context.Reports
                .AsNoTracking()
                .Where(r => r.Id == id && r.ReporterId == userId)
                .Select(r => new ReportDto
                {
                    Id = r.Id,
                    ReporterUsername = r.Reporter.UserName ?? "",
                    ReportedEntityType = r.ReportedEntityType,
                    ReportedEntityId = r.ReportedEntityId,
                    ReportedUsername = r.ReportedUser != null ? r.ReportedUser.UserName : null,
                    Reason = r.Reason,
                    Details = r.Details,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    ReviewedAt = r.ReviewedAt,
                    ReviewedByAdminUsername = r.ReviewedByAdmin != null ? r.ReviewedByAdmin.UserName : null,
                    AdminNotes = r.AdminNotes
                })
                .FirstOrDefaultAsync();

            if (report == null)
                return NotFound();

            return Ok(report);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var report = await _context.Reports
                .FirstOrDefaultAsync(r => r.Id == id && r.ReporterId == userId);

            if (report == null)
                return NotFound();

            // Only allow deletion if report is still pending
            if (report.Status != ReportStatusConstants.Pending)
                return BadRequest("Cannot delete a report that has been reviewed");

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
