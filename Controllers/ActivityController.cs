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

            var eventsCreated = await _context.Events
                .Where(e => friendIds.Contains(e.OrganizerId) && e.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                .Include(e => e.Organizer)
                .OrderByDescending(e => e.CreatedAt)
                .Take(20)
                .Select(e => new ActivityDto
                {
                    ActivityType = "EventCreated",
                    UserId = e.OrganizerId,
                    Username = e.Organizer.UserName,
                    DisplayName = _context.UserProfiles
                        .Where(up => up.UserId == e.OrganizerId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    Timestamp = e.CreatedAt,
                    EventId = e.Id,
                    EventTitle = e.Title,
                    EventType = e.EventType
                })
                .ToListAsync();

            activities.AddRange(eventsCreated);

            var eventRsvps = await _context.EventAttendees
                .Where(ea => friendIds.Contains(ea.UserId) &&
                             ea.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                .Include(ea => ea.User)
                .Include(ea => ea.Event)
                .OrderByDescending(ea => ea.CreatedAt)
                .Take(20)
                .Select(ea => new ActivityDto
                {
                    ActivityType = "EventRSVP",
                    UserId = ea.UserId,
                    Username = ea.User.UserName,
                    DisplayName = _context.UserProfiles
                        .Where(up => up.UserId == ea.UserId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    Timestamp = ea.CreatedAt,
                    EventId = ea.EventId,
                    EventTitle = ea.Event.Title,
                    EventType = ea.Event.EventType,
                    RsvpStatus = ea.Status
                })
                .ToListAsync();

            activities.AddRange(eventRsvps);

            var interestsAdded = await _context.UserInterests
                .Where(ui => friendIds.Contains(ui.UserId) && ui.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                .Include(ui => ui.User)
                .Include(ui => ui.SubInterest)
                    .ThenInclude(si => si.Interest)
                .OrderByDescending(ui => ui.CreatedAt)
                .Take(20)
                .Select(ui => new ActivityDto
                {
                    ActivityType = "InterestAdded",
                    UserId = ui.UserId,
                    Username = ui.User.UserName,
                    DisplayName = _context.UserProfiles
                        .Where(up => up.UserId == ui.UserId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    Timestamp = ui.CreatedAt,
                    SubInterestId = ui.SubInterestId,
                    SubInterestName = ui.SubInterest.Name,
                    InterestName = ui.SubInterest.Interest.Name
                })
                .ToListAsync();

            activities.AddRange(interestsAdded);

            var sortedActivities = activities
                .OrderByDescending(a => a.Timestamp)
                .Take(50)
                .ToList();

            return Ok(sortedActivities);
        }
    }
}
