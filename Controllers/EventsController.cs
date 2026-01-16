using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Diversion.Constants;
using Diversion.DTOs;
using Diversion.Models;
using Diversion.Helpers;

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
                ZipCode = e.ZipCode,
                MeetingUrl = e.MeetingUrl,
                RequiresRsvp = e.RequiresRsvp,
                TicketPrice = e.TicketPrice,
                MaxAttendees = e.MaxAttendees,
                MinAge = e.MinAge,
                MaxAge = e.MaxAge,
                CreatedAt = e.CreatedAt
            };
        }

        /// <summary>
        /// Retrieves a paginated list of events
        /// </summary>
        /// <param name="skip">Number of records to skip for pagination (default: 0)</param>
        /// <param name="take">Number of records to return (default: 50, max: 100)</param>
        /// <returns>A list of events excluding those from blocked or banned users</returns>
        /// <response code="200">Returns the list of events</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Limit maximum page size
            if (take > 100)
                take = 100;

            // Get blocked and banned user IDs
            var excludedUserIds = !string.IsNullOrEmpty(userId)
                ? await UserFilterHelper.GetExcludedUserIdsAsync(_context, userId)
                : new HashSet<string>();

            var events = await _context.Events
                .AsNoTracking()
                .Include(e => e.InterestTag)
                .Include(e => e.Organizer)
                .Where(e => !excludedUserIds.Contains(e.OrganizerId))
                .OrderByDescending(e => e.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return Ok(events.Select(MapEventToDto));
        }

        /// <summary>
        /// Retrieves detailed information for a specific event
        /// </summary>
        /// <param name="id">The unique identifier of the event</param>
        /// <returns>Detailed event information including attendee list</returns>
        /// <response code="200">Returns the event details</response>
        /// <response code="404">Event not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EventDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EventDetailDto>> GetEvent(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get blocked user IDs
            var blockedUserIds = !string.IsNullOrEmpty(userId)
                ? await UserFilterHelper.GetBlockedUserIdsAsync(_context, userId)
                : new List<string>();

            var eventDetail = await _context.Events
                .AsNoTracking()
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
                    ZipCode = e.ZipCode,
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
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEventsByInterest(
            Guid interestTagId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Limit maximum page size
            if (take > 100)
                take = 100;

            // Get blocked and banned user IDs
            var excludedUserIds = !string.IsNullOrEmpty(userId)
                ? await UserFilterHelper.GetExcludedUserIdsAsync(_context, userId)
                : new HashSet<string>();

            var events = await _context.Events
                .AsNoTracking()
                .Include(e => e.InterestTag)
                .Include(e => e.Organizer)
                .Where(e => e.InterestTagId == interestTagId && !excludedUserIds.Contains(e.OrganizerId))
                .OrderByDescending(e => e.StartDateTime)
                .Skip(skip)
                .Take(take)
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
                .AsNoTracking()
                .Include(e => e.InterestTag)
                .Include(e => e.Organizer)
                .Where(e => e.OrganizerId == userId)
                .ToListAsync();

            return Ok(events.Select(MapEventToDto));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetUserEvents(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if users are blocked
            if (!string.IsNullOrEmpty(currentUserId) && currentUserId != userId)
            {
                var areBlocked = await _context.UserBlocks
                    .AsNoTracking()
                    .AnyAsync(ub =>
                        (ub.BlockerId == currentUserId && ub.BlockedUserId == userId) ||
                        (ub.BlockerId == userId && ub.BlockedUserId == currentUserId));

                if (areBlocked)
                    return Ok(new List<EventDto>()); // Return empty list instead of forbidding
            }

            var events = await _context.Events
                .AsNoTracking()
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
                .AsNoTracking()
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

        /// <summary>
        /// Creates a new event
        /// </summary>
        /// <param name="dto">Event creation data including title, description, dates, location, and settings</param>
        /// <returns>The created event</returns>
        /// <remarks>
        /// Supports caregiver "acting on behalf of" functionality via ActingOnBehalfOf property.
        /// Online events require MeetingUrl; in-person events require City and State.
        /// Start date must be in the future, and end date must be after start date.
        /// </remarks>
        /// <response code="201">Event created successfully</response>
        /// <response code="400">Invalid request data or validation failure</response>
        /// <response code="401">Unauthorized - authentication required</response>
        [HttpPost]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Validate caregiver action if acting on behalf
            var authResult = await CaregiverAuthHelper.ValidateAndAuthorize(
                _context, userId, dto.ActingOnBehalfOf, CaregiverPermission.ManageEvents);

            if (!authResult.IsAuthorized)
                return BadRequest(authResult.ErrorMessage);

            // Use effective user ID (recipient if caregiver, otherwise self)
            var effectiveUserId = authResult.EffectiveUserId;

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
                    OrganizerId = effectiveUserId,
                    InterestTagId = dto.InterestTagId,
                    Title = dto.Title,
                    Description = dto.Description,
                    StartDateTime = dto.StartDateTime,
                    EndDateTime = dto.EndDateTime,
                    EventType = dto.EventType,
                    StreetAddress = dto.StreetAddress,
                    City = dto.City,
                    State = dto.State,
                    ZipCode = dto.ZipCode,
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
                    UserId = effectiveUserId,
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
            if (dto.ZipCode != null)
                eventToUpdate.ZipCode = dto.ZipCode;
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

        [HttpGet("nearby")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetNearbyEvents(
            [FromQuery] string zipCode)
        {
            if (string.IsNullOrWhiteSpace(zipCode))
                return BadRequest("Zip code is required");

            // Validate zip code format (5 digits)
            if (zipCode.Length < 3 || !zipCode.All(char.IsDigit))
                return BadRequest("Invalid zip code format");

            // Get first 3 digits for proximity matching (same ~50 mile area)
            var zipPrefix = zipCode.Substring(0, 3);

            // Query in-person events with matching zip code prefix
            var events = await _context.Events
                .AsNoTracking()
                .Include(e => e.InterestTag)
                .Include(e => e.Organizer)
                .Where(e => e.EventType == EventTypeConstants.InPerson
                         && e.ZipCode != null
                         && e.ZipCode.StartsWith(zipPrefix)
                         && e.StartDateTime > DateTime.UtcNow)
                .OrderBy(e => e.StartDateTime)
                .Take(50)
                .ToListAsync();

            var result = events.Select(e => MapEventToDto(e)).ToList();
            return Ok(result);
        }
    }
}