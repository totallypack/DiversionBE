using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Diversion.DTOs;
using Diversion.Hubs;
using Diversion.Models;
using Diversion.Helpers;
using Diversion.Constants;

namespace Diversion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventAttendeesController(DiversionDbContext context, IHubContext<NotificationHub> notificationHub) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;
        private readonly IHubContext<NotificationHub> _notificationHub = notificationHub;

        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<IEnumerable<EventAttendeeDto>>> GetEventAttendees(Guid eventId)
        {
            var attendees = await _context.EventAttendees
                .AsNoTracking()
                .Where(ea => ea.EventId == eventId)
                .Select(ea => new EventAttendeeDto
                {
                    Id = ea.Id,
                    EventId = ea.EventId,
                    UserId = ea.UserId,
                    Username = ea.User.UserName ?? "",
                    Status = ea.Status,
                    CreatedAt = ea.CreatedAt
                })
                .ToListAsync();

            return Ok(attendees);
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<EventAttendeeDto>>> GetMyAttendance()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var attendees = await _context.EventAttendees
                .AsNoTracking()
                .Where(ea => ea.UserId == userId)
                .Select(ea => new EventAttendeeDto
                {
                    Id = ea.Id,
                    EventId = ea.EventId,
                    UserId = ea.UserId,
                    Username = ea.User.UserName ?? "",
                    Status = ea.Status,
                    CreatedAt = ea.CreatedAt
                })
                .ToListAsync();

            return Ok(attendees);
        }

        [HttpGet("event/{eventId}/me")]
        public async Task<ActionResult<EventAttendeeDto?>> GetMyAttendanceForEvent(Guid eventId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var attendee = await _context.EventAttendees
                .AsNoTracking()
                .Where(ea => ea.EventId == eventId && ea.UserId == userId)
                .Select(ea => new EventAttendeeDto
                {
                    Id = ea.Id,
                    EventId = ea.EventId,
                    UserId = ea.UserId,
                    Username = ea.User.UserName ?? "",
                    Status = ea.Status,
                    CreatedAt = ea.CreatedAt
                })
                .FirstOrDefaultAsync();

            return Ok(attendee);
        }

        [HttpPost]
        public async Task<ActionResult<EventAttendeeDto>> RsvpToEvent([FromBody] CreateEventAttendeeDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var eventToRsvp = await _context.Events.FindAsync(dto.EventId);
            if (eventToRsvp == null)
                return BadRequest("Event not found");

            if (eventToRsvp.OrganizerId == userId)
                return BadRequest("Event organizers are automatically added as attendees");

            var existingAttendee = await _context.EventAttendees
                .FirstOrDefaultAsync(ea => ea.EventId == dto.EventId && ea.UserId == userId);

            if (existingAttendee != null)
                return BadRequest("Already RSVPed to this event");

            var attendee = new EventAttendee
            {
                Id = Guid.NewGuid(),
                EventId = dto.EventId,
                UserId = userId,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.EventAttendees.Add(attendee);
            await _context.SaveChangesAsync();

            // Notify event organizer if someone RSVPs "Going"
            if (dto.Status == AttendeeStatusConstants.Going && userId != eventToRsvp.OrganizerId)
            {
                var attendeeUser = await _context.Users.FindAsync(userId);
                await NotificationHelper.NotifyEventRSVPAsync(
                    _context,
                    eventToRsvp.OrganizerId,
                    attendeeUser?.UserName ?? "Someone",
                    eventToRsvp.Id.ToString(),
                    eventToRsvp.Title,
                    _notificationHub);
            }

            var result = await _context.EventAttendees
                .AsNoTracking()
                .Where(ea => ea.Id == attendee.Id)
                .Select(ea => new EventAttendeeDto
                {
                    Id = ea.Id,
                    EventId = ea.EventId,
                    UserId = ea.UserId,
                    Username = ea.User.UserName ?? "",
                    Status = ea.Status,
                    CreatedAt = ea.CreatedAt
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetMyAttendanceForEvent), new { eventId = dto.EventId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRsvp(Guid id, [FromBody] UpdateEventAttendeeDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var attendee = await _context.EventAttendees
                .Include(ea => ea.Event)
                .FirstOrDefaultAsync(ea => ea.Id == id && ea.UserId == userId);

            if (attendee == null)
                return NotFound();

            var previousStatus = attendee.Status;
            attendee.Status = dto.Status;

            await _context.SaveChangesAsync();

            // Notify event organizer if status changes to "Going" and wasn't "Going" before
            if (dto.Status == AttendeeStatusConstants.Going &&
                previousStatus != AttendeeStatusConstants.Going &&
                userId != attendee.Event.OrganizerId)
            {
                var attendeeUser = await _context.Users.FindAsync(userId);
                await NotificationHelper.NotifyEventRSVPAsync(
                    _context,
                    attendee.Event.OrganizerId,
                    attendeeUser?.UserName ?? "Someone",
                    attendee.Event.Id.ToString(),
                    attendee.Event.Title,
                    _notificationHub);
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRsvp(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var attendee = await _context.EventAttendees
                .Include(ea => ea.Event)
                .FirstOrDefaultAsync(ea => ea.Id == id && ea.UserId == userId);

            if (attendee == null)
                return NotFound();

            if (attendee.Event.OrganizerId == userId)
                return BadRequest("Event organizers cannot remove their attendance");

            _context.EventAttendees.Remove(attendee);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}