using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Diversion.DTOs;
using Diversion.Models;

namespace Diversion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventsController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        // GET: api/events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.InterestTag)
                .Include(e => e.Organizer)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    OrganizerId = e.OrganizerId,
                    OrganizerUsername = e.Organizer.UserName,
                    InterestTagId = e.InterestTagId,
                    InterestTagName = e.InterestTag.Name,
                    Title = e.Title,
                    Description = e.Description,
                    StartDateTime = e.StartDateTime,
                    EndDateTime = e.EndDateTime,
                    EventType = e.EventType,
                    Location = e.Location,
                    MeetingUrl = e.MeetingUrl,
                    RequiresRsvp = e.RequiresRsvp,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<EventDetailDto>> GetEvent(Guid id)
        {
            var eventDetail = await _context.Events
                .Include(e => e.InterestTag)
                    .ThenInclude(it => it.Interest)
                .Include(e => e.Organizer)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.User)
                .Where(e => e.Id == id)
                .Select(e => new EventDetailDto
                {
                    Id = e.Id,
                    OrganizerId = e.OrganizerId,
                    OrganizerUsername = e.Organizer.UserName,
                    InterestTag = new SubInterestDto
                    {
                        Id = e.InterestTag.Id,
                        Name = e.InterestTag.Name,
                        InterestId = e.InterestTag.InterestId,
                        Description = e.InterestTag.Description,
                        IconUrl = e.InterestTag.IconUrl
                    },
                    Title = e.Title,
                    Description = e.Description,
                    StartDateTime = e.StartDateTime,
                    EndDateTime = e.EndDateTime,
                    EventType = e.EventType,
                    Location = e.Location,
                    MeetingUrl = e.MeetingUrl,
                    RequiresRsvp = e.RequiresRsvp,
                    CreatedAt = e.CreatedAt,
                    Attendees = e.Attendees.Select(a => new EventAttendeeDto
                    {
                        Id = a.Id,
                        EventId = a.EventId,
                        UserId = a.UserId,
                        Username = a.User.UserName,
                        Status = a.Status,
                        CreatedAt = a.CreatedAt
                    }).ToList(),
                    AttendeeCount = e.Attendees.Count(a => a.Status == "Going")
                })
                .FirstOrDefaultAsync();

            if (eventDetail == null)
                return NotFound();

            return Ok(eventDetail);
        }

        // GET: api/events/interest/{interestTagId}
        [HttpGet("interest/{interestTagId}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEventsByInterest(Guid interestTagId)
        {
            var events = await _context.Events
                .Include(e => e.InterestTag)
                .Include(e => e.Organizer)
                .Where(e => e.InterestTagId == interestTagId)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    OrganizerId = e.OrganizerId,
                    OrganizerUsername = e.Organizer.UserName,
                    InterestTagId = e.InterestTagId,
                    InterestTagName = e.InterestTag.Name,
                    Title = e.Title,
                    Description = e.Description,
                    StartDateTime = e.StartDateTime,
                    EndDateTime = e.EndDateTime,
                    EventType = e.EventType,
                    Location = e.Location,
                    MeetingUrl = e.MeetingUrl,
                    RequiresRsvp = e.RequiresRsvp,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/my
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetMyEvents()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var events = await _context.Events
                .Include(e => e.InterestTag)
                .Include(e => e.Organizer)
                .Where(e => e.OrganizerId == userId)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    OrganizerId = e.OrganizerId,
                    OrganizerUsername = e.Organizer.UserName,
                    InterestTagId = e.InterestTagId,
                    InterestTagName = e.InterestTag.Name,
                    Title = e.Title,
                    Description = e.Description,
                    StartDateTime = e.StartDateTime,
                    EndDateTime = e.EndDateTime,
                    EventType = e.EventType,
                    Location = e.Location,
                    MeetingUrl = e.MeetingUrl,
                    RequiresRsvp = e.RequiresRsvp,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return Ok(events);
        }

        // POST: api/events
        [HttpPost]
        public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Validate interest tag exists
            var interestTagExists = await _context.SubInterests
                .AnyAsync(si => si.Id == dto.InterestTagId);

            if (!interestTagExists)
                return BadRequest("Interest tag not found");

            // Validate dates
            if (dto.EndDateTime <= dto.StartDateTime)
                return BadRequest("End date must be after start date");

            // Validate future dates
            if (dto.StartDateTime <= DateTime.UtcNow)
                return BadRequest("Start date must be in the future");

            // Validate event type specific fields
            if (dto.EventType == "Online" && string.IsNullOrWhiteSpace(dto.MeetingUrl))
                return BadRequest("Meeting URL is required for online events");

            if (dto.EventType == "InPerson" && string.IsNullOrWhiteSpace(dto.Location))
                return BadRequest("Location is required for in-person events");

            var newEvent = new Event
            {
                Id = Guid.NewGuid(),
                OrganizerId = userId,
                InterestTagId = dto.InterestTagId,
                Title = dto.Title,
                Description = dto.Description,
                StartDateTime = dto.StartDateTime,
                EndDateTime = dto.EndDateTime,
                EventType = dto.EventType,
                Location = dto.Location,
                MeetingUrl = dto.MeetingUrl,
                RequiresRsvp = dto.RequiresRsvp,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            var result = await _context.Events
                .Include(e => e.InterestTag)
                .Include(e => e.Organizer)
                .Where(e => e.Id == newEvent.Id)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    OrganizerId = e.OrganizerId,
                    OrganizerUsername = e.Organizer.UserName,
                    InterestTagId = e.InterestTagId,
                    InterestTagName = e.InterestTag.Name,
                    Title = e.Title,
                    Description = e.Description,
                    StartDateTime = e.StartDateTime,
                    EndDateTime = e.EndDateTime,
                    Location = e.Location,
                    RequiresRsvp = e.RequiresRsvp,
                    CreatedAt = e.CreatedAt
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = newEvent.Id }, result);
        }

        // PUT: api/events/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var eventToUpdate = await _context.Events.FindAsync(id);

            if (eventToUpdate == null)
                return NotFound();

            if (eventToUpdate.OrganizerId != userId)
                return Forbid();

            if (dto.InterestTagId.HasValue)
            {
                var interestTagExists = await _context.SubInterests
                    .AnyAsync(si => si.Id == dto.InterestTagId.Value);

                if (!interestTagExists)
                    return BadRequest("Interest tag not found");

                eventToUpdate.InterestTagId = dto.InterestTagId.Value;
            }

            if (dto.Title != null)
                eventToUpdate.Title = dto.Title;
            if (dto.Description != null)
                eventToUpdate.Description = dto.Description;
            if (dto.StartDateTime.HasValue)
                eventToUpdate.StartDateTime = dto.StartDateTime.Value;
            if (dto.EndDateTime.HasValue)
                eventToUpdate.EndDateTime = dto.EndDateTime.Value;
            if (dto.EventType != null)
                eventToUpdate.EventType = dto.EventType;
            if (dto.Location != null)
                eventToUpdate.Location = dto.Location;
            if (dto.MeetingUrl != null)
                eventToUpdate.MeetingUrl = dto.MeetingUrl;
            if (dto.RequiresRsvp.HasValue)
                eventToUpdate.RequiresRsvp = dto.RequiresRsvp.Value;

            // Validate dates
            if (eventToUpdate.EndDateTime <= eventToUpdate.StartDateTime)
                return BadRequest("End date must be after start date");

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/events/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var eventToDelete = await _context.Events.FindAsync(id);

            if (eventToDelete == null)
                return NotFound();

            if (eventToDelete.OrganizerId != userId)
                return Forbid();

            _context.Events.Remove(eventToDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}