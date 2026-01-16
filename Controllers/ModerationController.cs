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
    public class ModerationController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        // Helper method to check if user is admin
        private async Task<bool> IsUserAdminAsync(string userId)
        {
            var userProfile = await _context.UserProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(up => up.UserId == userId);
            return userProfile?.IsAdmin ?? false;
        }

        [HttpGet("reports/pending")]
        public async Task<ActionResult<IEnumerable<ReportDto>>> GetPendingReports()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!await IsUserAdminAsync(userId))
                return Forbid();

            var reports = await _context.Reports
                .AsNoTracking()
                .Where(r => r.Status == ReportStatusConstants.Pending || r.Status == ReportStatusConstants.UnderReview)
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
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            return Ok(reports);
        }

        [HttpGet("reports/all")]
        public async Task<ActionResult<IEnumerable<ReportDto>>> GetAllReports(
            [FromQuery] string? status = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!await IsUserAdminAsync(userId))
                return Forbid();

            if (take > 100)
                take = 100;

            var query = _context.Reports.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
            }

            var reports = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
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
                .ToListAsync();

            return Ok(reports);
        }

        [HttpPost("reports/{reportId}/review")]
        public async Task<ActionResult<ReportDto>> ReviewReport(int reportId, [FromBody] ReportReviewDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!await IsUserAdminAsync(userId))
                return Forbid();

            var report = await _context.Reports
                .Include(r => r.ReportedUser)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
                return NotFound("Report not found");

            if (report.Status != ReportStatusConstants.Pending && report.Status != ReportStatusConstants.UnderReview)
                return BadRequest("Report has already been reviewed");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Update report status
                report.Status = dto.Status;
                report.ReviewedAt = DateTime.UtcNow;
                report.ReviewedByAdminId = userId;
                report.AdminNotes = dto.AdminNotes;

                // Create moderation action if specified
                if (!string.IsNullOrWhiteSpace(dto.ActionType))
                {
                    var moderationAction = new ModerationAction
                    {
                        AdminId = userId,
                        ActionType = dto.ActionType,
                        TargetUserId = report.ReportedUserId,
                        TargetEntityType = report.ReportedEntityType,
                        TargetEntityId = report.ReportedEntityId,
                        RelatedReportId = reportId,
                        Reason = dto.ActionReason ?? dto.AdminNotes ?? report.Reason,
                        Notes = dto.AdminNotes,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.ModerationActions.Add(moderationAction);

                    // Execute the action
                    switch (dto.ActionType)
                    {
                        case ModerationActionTypeConstants.BanUser:
                            if (!string.IsNullOrEmpty(report.ReportedUserId))
                            {
                                var userProfile = await _context.UserProfiles
                                    .FirstOrDefaultAsync(up => up.UserId == report.ReportedUserId);
                                if (userProfile != null)
                                {
                                    userProfile.IsBanned = true;
                                    userProfile.BannedAt = DateTime.UtcNow;
                                    userProfile.BanReason = dto.ActionReason ?? "Violation of community guidelines";
                                }
                            }
                            break;

                        case ModerationActionTypeConstants.DeleteEvent:
                            var evt = await _context.Events.FindAsync(Guid.Parse(report.ReportedEntityId));
                            if (evt != null)
                            {
                                _context.Events.Remove(evt);
                            }
                            break;

                        case ModerationActionTypeConstants.DeleteMessage:
                            if (report.ReportedEntityType == "CommunityMessage")
                            {
                                var communityMsg = await _context.CommunityMessages
                                    .FindAsync(int.Parse(report.ReportedEntityId));
                                if (communityMsg != null)
                                {
                                    _context.CommunityMessages.Remove(communityMsg);
                                }
                            }
                            else if (report.ReportedEntityType == "DirectMessage")
                            {
                                var directMsg = await _context.DirectMessages
                                    .FindAsync(Guid.Parse(report.ReportedEntityId));
                                if (directMsg != null)
                                {
                                    _context.DirectMessages.Remove(directMsg);
                                }
                            }
                            break;

                        case ModerationActionTypeConstants.WarnUser:
                            // Warning is just recorded in ModerationAction, no other action needed
                            break;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = new ReportDto
                {
                    Id = report.Id,
                    ReporterUsername = (await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == report.ReporterId))?.UserName ?? "",
                    ReportedEntityType = report.ReportedEntityType,
                    ReportedEntityId = report.ReportedEntityId,
                    ReportedUsername = report.ReportedUser?.UserName,
                    Reason = report.Reason,
                    Details = report.Details,
                    Status = report.Status,
                    CreatedAt = report.CreatedAt,
                    ReviewedAt = report.ReviewedAt,
                    ReviewedByAdminUsername = (await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId))?.UserName ?? "",
                    AdminNotes = report.AdminNotes
                };

                return Ok(result);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpPost("users/{targetUserId}/ban")]
        public async Task<IActionResult> BanUser(string targetUserId, [FromBody] BanUserDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!await IsUserAdminAsync(userId))
                return Forbid();

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == targetUserId);

            if (userProfile == null)
                return NotFound("User not found");

            if (userProfile.IsBanned)
                return BadRequest("User is already banned");

            // Don't allow banning other admins
            if (userProfile.IsAdmin)
                return BadRequest("Cannot ban an admin");

            userProfile.IsBanned = true;
            userProfile.BannedAt = DateTime.UtcNow;
            userProfile.BanReason = dto.Reason ?? "Violation of community guidelines";

            // Create moderation action
            var moderationAction = new ModerationAction
            {
                AdminId = userId,
                ActionType = ModerationActionTypeConstants.BanUser,
                TargetUserId = targetUserId,
                Reason = dto.Reason ?? "Violation of community guidelines",
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.ModerationActions.Add(moderationAction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("users/{targetUserId}/unban")]
        public async Task<IActionResult> UnbanUser(string targetUserId, [FromBody] UnbanUserDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!await IsUserAdminAsync(userId))
                return Forbid();

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == targetUserId);

            if (userProfile == null)
                return NotFound("User not found");

            if (!userProfile.IsBanned)
                return BadRequest("User is not banned");

            userProfile.IsBanned = false;
            userProfile.BannedAt = null;
            userProfile.BanReason = null;

            // Create moderation action
            var moderationAction = new ModerationAction
            {
                AdminId = userId,
                ActionType = ModerationActionTypeConstants.UnbanUser,
                TargetUserId = targetUserId,
                Reason = dto.Reason ?? "Ban lifted",
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.ModerationActions.Add(moderationAction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("actions")]
        public async Task<ActionResult<IEnumerable<object>>> GetModerationActions(
            [FromQuery] string? targetUserId = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!await IsUserAdminAsync(userId))
                return Forbid();

            if (take > 100)
                take = 100;

            var query = _context.ModerationActions.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(targetUserId))
            {
                query = query.Where(ma => ma.TargetUserId == targetUserId);
            }

            var actions = await query
                .OrderByDescending(ma => ma.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(ma => new
                {
                    ma.Id,
                    AdminUsername = ma.Admin.UserName ?? "",
                    ma.ActionType,
                    TargetUsername = ma.TargetUser != null ? ma.TargetUser.UserName : null,
                    ma.TargetEntityType,
                    ma.TargetEntityId,
                    ma.RelatedReportId,
                    ma.Reason,
                    ma.Notes,
                    ma.CreatedAt
                })
                .ToListAsync();

            return Ok(actions);
        }

        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetModerationStats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!await IsUserAdminAsync(userId))
                return Forbid();

            var pendingReportsCount = await _context.Reports
                .AsNoTracking()
                .CountAsync(r => r.Status == ReportStatusConstants.Pending);

            var underReviewReportsCount = await _context.Reports
                .AsNoTracking()
                .CountAsync(r => r.Status == ReportStatusConstants.UnderReview);

            var totalReportsCount = await _context.Reports.AsNoTracking().CountAsync();

            var bannedUsersCount = await _context.UserProfiles
                .AsNoTracking()
                .CountAsync(up => up.IsBanned);

            var moderationActionsLast30Days = await _context.ModerationActions
                .AsNoTracking()
                .CountAsync(ma => ma.CreatedAt >= DateTime.UtcNow.AddDays(-30));

            return Ok(new
            {
                pendingReportsCount,
                underReviewReportsCount,
                totalReportsCount,
                bannedUsersCount,
                moderationActionsLast30Days
            });
        }
    }

    // DTOs for ban/unban operations
    public class BanUserDto
    {
        public string? Reason { get; set; }
        public string? Notes { get; set; }
    }

    public class UnbanUserDto
    {
        public string? Reason { get; set; }
        public string? Notes { get; set; }
    }
}
