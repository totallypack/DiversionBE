using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Diversion.Constants;
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

        private EventDto MapEventToDto(Event e)
        {
            return new EventDto
            {
                Id = e.Id,
                OrganizerId = e.OrganizerId,
                OrganizerUsername = e.Organizer.UserName ?? "",
                InterestTagId = e.InterestTagId,
                InterestTagName = e.InterestTag.Name,
                Title = e.Title,
                Description = e.Description,
                StartDateTime = e.StartDateTime,
                EndDateTime = e.EndDateTime,
                EventType = e.EventType,
                StreetAddress = e.StreetAddress,
                City = e.City,
                State = e.State,
                MeetingUrl = e.MeetingUrl,
                RequiresRsvp = e.RequiresRsvp,
                TicketPrice = e.TicketPrice,
                MaxAttendees = e.MaxAttendees,
                MinAge = e.MinAge,
                MaxAge = e.MaxAge,
                CreatedAt = e.CreatedAt
            };
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.InterestTag)
                .Include(e => e.Organizer)
                .ToListAsync();

            return Ok(events.Select(MapEventToDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventDetailDto>> GetEvent(Guid id)
        {
            var eventDetail = await _context.Events
                .Where(e => e.Id == id)
                .Select(e => new EventDetailDto
                {
                    Id = e.Id,
                    OrganizerId = e.OrganizerId,
                    OrganizerUsername = e.Organizer.UserName ?? "",
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
                    StreetAddress = e.StreetAddress,
                    City = e.City,
                    State = e.State,
                    MeetingUrl = e.MeetingUrl,
                    RequiresRsvp = e.RequiresRsvp,
                    TicketPrice = e.TicketPrice,
                    MaxAttendees = e.MaxAttendees,
                    MinAge = e.MinAge,
                    MaxAge = e.MaxAge,
                    CreatedAt = e.CreatedAt,
                    Attendees = e.Attendees.Select(a => new EventAttendeeDto
                    {
                        Id = a.Id,
                        EventId = a.EventId,
                        UserId = a.UserId,
                        Username = a.User.UserName ?? "",
                        Status = a.Status,
                        CreatedAt = a.CreatedAt
                    }).ToList(),
                    AttendeeCount = e.Attendees.Count(a => a.Status == AttendeeStatusConstants.Going)
                })
                .FirstOrDefaultAsync();

            if (eventDetail == null)
                return NotFound();

            return Ok(eventDetail);
        }

        [HttpGet("interest/{interestTagId}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEventsByInterest(Guid interestTagId)
        {
            var events = await _context.Events
                .Include(e => e.InterestTag)
                .Include(e => e.Organizer)
                .Where(e => e.InterestTagId == interestTagId)
                .ToListAsync();

            return Ok(events.Select(MapEventToDto));
        }

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
                .ToListAsync();

            return Ok(events.Select(MapEventToDto));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetUserEvents(string userId)
        {
            var events = await _context.Events
                .Include(e => e.InterestTag)
                .Include(e => e.Organizer)
                .Where(e => e.OrganizerId == userId)
                .ToListAsync();

            return Ok(events.Select(MapEventToDto));
        }

        [HttpGet("rsvpd")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetMyRsvpdEvents()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var eventAttendees = await _context.EventAttendees
                .Include(ea => ea.Event)
                    .ThenInclude(e => e.InterestTag)
                .Include(ea => ea.Event)
                    .ThenInclude(e => e.Organizer)
                .Where(ea => ea.UserId == userId)
                .OrderByDescending(ea => ea.Event.StartDateTime)
                .ToListAsync();

            var events = eventAttendees.Select(ea =>
            {
                var dto = MapEventToDto(ea.Event);
                dto.RsvpStatus = ea.Status;
                return dto;
            });

            return Ok(events);
        }

        [HttpPost]
        public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var interestTagExists = await _context.SubInterests
                .AnyAsync(si => si.Id == dto.InterestTagId);

            if (!interestTagExists)
                return BadRequest("Interest tag not found");

            if (dto.EndDateTime <= dto.StartDateTime)
                return BadRequest("End date must be after start date");

            if (dto.StartDateTime <= DateTime.UtcNow)
                return BadRequest("Start date must be in the future");

            if (dto.EventType == EventTypeConstants.Online && string.IsNullOrWhiteSpace(dto.MeetingUrl))
                return BadRequest("Meeting URL is required for online events");

            if (dto.EventType == EventTypeConstants.InPerson && (string.IsNullOrWhiteSpace(dto.City) || string.IsNullOrWhiteSpace(dto.State)))
                return BadRequest("City and State are required for in-person events");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
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
                    StreetAddress = dto.StreetAddress,
                    City = dto.City,
                    State = dto.State,
                    MeetingUrl = dto.MeetingUrl,
                    RequiresRsvp = dto.RequiresRsvp,
                    TicketPrice = dto.TicketPrice,
                    MaxAttendees = dto.MaxAttendees,
                    MinAge = dto.MinAge,
                    MaxAge = dto.MaxAge,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Events.Add(newEvent);

                var organizerAttendee = new EventAttendee
                {
                    Id = Guid.NewGuid(),
                    EventId = newEvent.Id,
                    UserId = userId,
                    Status = AttendeeStatusConstants.Going,
                    CreatedAt = DateTime.UtcNow
                };

                _context.EventAttendees.Add(organizerAttendee);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var createdEvent = await _context.Events
                    .Include(e => e.InterestTag)
                    .Include(e => e.Organizer)
                    .FirstOrDefaultAsync(e => e.Id == newEvent.Id);

                var result = MapEventToDto(createdEvent);

                return CreatedAtAction(nameof(GetEvent), new { id = newEvent.Id }, result);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

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
            if (dto.StreetAddress != null)
                eventToUpdate.StreetAddress = dto.StreetAddress;
            if (dto.City != null)
                eventToUpdate.City = dto.City;
            if (dto.State != null)
                eventToUpdate.State = dto.State;
            if (dto.MeetingUrl != null)
                eventToUpdate.MeetingUrl = dto.MeetingUrl;
            if (dto.RequiresRsvp.HasValue)
                eventToUpdate.RequiresRsvp = dto.RequiresRsvp.Value;
            if (dto.TicketPrice.HasValue)
                eventToUpdate.TicketPrice = dto.TicketPrice.Value;
            if (dto.MaxAttendees.HasValue)
                eventToUpdate.MaxAttendees = dto.MaxAttendees.Value;
            if (dto.MinAge.HasValue)
                eventToUpdate.MinAge = dto.MinAge.Value;
            if (dto.MaxAge.HasValue)
                eventToUpdate.MaxAge = dto.MaxAge.Value;

            if (eventToUpdate.EndDateTime <= eventToUpdate.StartDateTime)
                return BadRequest("End date must be after start date");

            await _context.SaveChangesAsync();

            return NoContent();
        }

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