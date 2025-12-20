using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Diversion.DTOs;

namespace Diversion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ActivityController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        private (string displayName, string profilePicUrl) GetUserProfile(string userId)
        {
            var profile = _context.UserProfiles
                .Where(up => up.UserId == userId)
                .Select(up => new { up.DisplayName, up.ProfilePicUrl })
                .FirstOrDefault();

            return (profile?.DisplayName, profile?.ProfilePicUrl);
        }

        [HttpGet("feed")]
        public async Task<ActionResult<IEnumerable<ActivityDto>>> GetFriendActivityFeed()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var friendIds = await _context.Friendships
                .Where(f => f.UserId == userId)
                .Select(f => f.FriendId)
                .ToListAsync();

            if (friendIds.Count == 0)
                return Ok(new List<ActivityDto>());

            var activities = new List<ActivityDto>();

            var events = await _context.Events
                .Where(e => friendIds.Contains(e.OrganizerId) && e.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                .Include(e => e.Organizer)
                .OrderByDescending(e => e.CreatedAt)
                .Take(20)
                .ToListAsync();

            var eventsCreated = events.Select(e =>
            {
                var (displayName, profilePicUrl) = GetUserProfile(e.OrganizerId);
                return new ActivityDto
                {
                    ActivityType = "EventCreated",
                    UserId = e.OrganizerId,
                    Username = e.Organizer.UserName ?? "",
                    DisplayName = displayName,
                    ProfilePicUrl = profilePicUrl,
                    Timestamp = e.CreatedAt,
                    EventId = e.Id,
                    EventTitle = e.Title,
                    EventType = e.EventType
                };
            });

            activities.AddRange(eventsCreated);

            var attendees = await _context.EventAttendees
                .Where(ea => friendIds.Contains(ea.UserId) &&
                             ea.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                .Include(ea => ea.User)
                .Include(ea => ea.Event)
                .OrderByDescending(ea => ea.CreatedAt)
                .Take(20)
                .ToListAsync();

            var eventRsvps = attendees.Select(ea =>
            {
                var (displayName, profilePicUrl) = GetUserProfile(ea.UserId);
                return new ActivityDto
                {
                    ActivityType = "EventRSVP",
                    UserId = ea.UserId,
                    Username = ea.User.UserName ?? "",
                    DisplayName = displayName,
                    ProfilePicUrl = profilePicUrl,
                    Timestamp = ea.CreatedAt,
                    EventId = ea.EventId,
                    EventTitle = ea.Event.Title,
                    EventType = ea.Event.EventType,
                    RsvpStatus = ea.Status
                };
            });

            activities.AddRange(eventRsvps);

            var userInterests = await _context.UserInterests
                .Where(ui => friendIds.Contains(ui.UserId) && ui.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                .Include(ui => ui.User)
                .Include(ui => ui.SubInterest)
                    .ThenInclude(si => si.Interest)
                .OrderByDescending(ui => ui.CreatedAt)
                .Take(20)
                .ToListAsync();

            var interestsAdded = userInterests.Select(ui =>
            {
                var (displayName, profilePicUrl) = GetUserProfile(ui.UserId);
                return new ActivityDto
                {
                    ActivityType = "InterestAdded",
                    UserId = ui.UserId,
                    Username = ui.User.UserName ?? "",
                    DisplayName = displayName,
                    ProfilePicUrl = profilePicUrl,
                    Timestamp = ui.CreatedAt,
                    SubInterestId = ui.SubInterestId,
                    SubInterestName = ui.SubInterest.Name,
                    InterestName = ui.SubInterest.Interest.Name
                };
            });

            activities.AddRange(interestsAdded);

            var sortedActivities = activities
                .OrderByDescending(a => a.Timestamp)
                .Take(50)
                .ToList();

            return Ok(sortedActivities);
        }
    }
}
