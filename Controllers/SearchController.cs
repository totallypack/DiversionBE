using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Diversion.DTOs;
using Diversion.Helpers;

namespace Diversion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SearchController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        [HttpGet]
        public async Task<ActionResult<SearchResultsDto>> Search(
            [FromQuery] string? query,
            [FromQuery] string? type = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(query))
                return Ok(new SearchResultsDto
                {
                    Events = [],
                    Users = [],
                    Communities = []
                });

            var searchTerm = $"%{query}%";

            // Get blocked and banned user IDs
            var excludedUserIds = await UserFilterHelper.GetExcludedUserIdsAsync(_context, userId);

            var results = new SearchResultsDto
            {
                Events = [],
                Users = [],
                Communities = []
            };

            // Search Events (if type is null or "events")
            if (type == null || type.ToLower() == "events")
            {
                results.Events = await _context.Events
                    .AsNoTracking()
                    .Where(e =>
                        (EF.Functions.Like(e.Title, searchTerm) ||
                         EF.Functions.Like(e.Description, searchTerm) ||
                         (e.City != null && EF.Functions.Like(e.City, searchTerm)) ||
                         (e.State != null && EF.Functions.Like(e.State, searchTerm))) &&
                        !excludedUserIds.Contains(e.OrganizerId) &&
                        e.StartDateTime >= DateTime.UtcNow)
                    .OrderBy(e => e.StartDateTime)
                    .Take(20)
                    .Select(e => new EventDto
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Description = e.Description,
                        StartDateTime = e.StartDateTime,
                        EndDateTime = e.EndDateTime,
                        EventType = e.EventType,
                        MeetingUrl = e.MeetingUrl,
                        StreetAddress = e.StreetAddress,
                        City = e.City,
                        State = e.State,
                        ZipCode = e.ZipCode,
                        OrganizerId = e.OrganizerId,
                        OrganizerUsername = e.Organizer.UserName ?? "",
                        InterestTagId = e.InterestTagId,
                        InterestTagName = e.InterestTag != null ? e.InterestTag.Name : null,
                        CreatedAt = e.CreatedAt,
                        RequiresRsvp = e.RequiresRsvp,
                        TicketPrice = e.TicketPrice,
                        MaxAttendees = e.MaxAttendees,
                        MinAge = e.MinAge,
                        MaxAge = e.MaxAge
                    })
                    .ToListAsync();
            }

            // Search Users (if type is null or "users")
            if (type == null || type.ToLower() == "users")
            {
                results.Users = await _context.UserProfiles
                    .AsNoTracking()
                    .Where(up =>
                        (EF.Functions.Like(up.User.UserName ?? "", searchTerm) ||
                         (up.DisplayName != null && EF.Functions.Like(up.DisplayName, searchTerm)) ||
                         (up.Bio != null && EF.Functions.Like(up.Bio, searchTerm)) ||
                         (up.City != null && EF.Functions.Like(up.City, searchTerm)) ||
                         (up.State != null && EF.Functions.Like(up.State, searchTerm))) &&
                        !excludedUserIds.Contains(up.UserId) &&
                        up.UserId != userId)
                    .Take(20)
                    .Select(up => new UserSearchDto
                    {
                        UserId = up.UserId,
                        Username = up.User.UserName ?? "",
                        DisplayName = up.DisplayName,
                        Bio = up.Bio,
                        ProfilePicUrl = up.ProfilePicUrl,
                        City = up.City,
                        State = up.State,
                        IsFriend = _context.Friendships
                            .Any(f => f.UserId == userId && f.FriendId == up.UserId)
                    })
                    .ToListAsync();
            }

            // Search Communities (if type is null or "communities")
            if (type == null || type.ToLower() == "communities")
            {
                results.Communities = await _context.Communities
                    .AsNoTracking()
                    .Where(c =>
                        (EF.Functions.Like(c.Name, searchTerm) ||
                         EF.Functions.Like(c.Description, searchTerm)) &&
                        !excludedUserIds.Contains(c.CreatorId) &&
                        (!c.IsPrivate || _context.CommunityMemberships
                            .Any(cm => cm.CommunityId == c.Id && cm.UserId == userId)))
                    .Take(20)
                    .Select(c => new CommunityDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        IsPrivate = c.IsPrivate,
                        CreatorId = c.CreatorId,
                        CreatorUsername = c.Creator.UserName ?? "",
                        CreatedAt = c.CreatedAt,
                        MemberCount = _context.CommunityMemberships
                            .Count(cm => cm.CommunityId == c.Id),
                        IsMember = _context.CommunityMemberships
                            .Any(cm => cm.CommunityId == c.Id && cm.UserId == userId)
                    })
                    .ToListAsync();
            }

            return Ok(results);
        }
    }
}
