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
    public class CaregiverRequestController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        [HttpPost("send")]
        public async Task<ActionResult<CaregiverRequestDto>> SendCaregiverRequest([FromBody] SendCaregiverRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Verify sender has Caregiver user type
            var senderProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (senderProfile == null || senderProfile.UserType != UserType.Caregiver)
                return BadRequest("Only users with Caregiver user type can send caregiver requests");

            if (userId == dto.RecipientId)
                return BadRequest("You cannot send a caregiver request to yourself");

            var recipientExists = await _context.Users.AnyAsync(u => u.Id == dto.RecipientId);
            if (!recipientExists)
                return BadRequest("User not found");

            // Check if active relationship already exists
            var activeRelationship = await _context.CareRelationships
                .AnyAsync(cr => cr.CaregiverId == userId && cr.RecipientId == dto.RecipientId && cr.IsActive);

            if (activeRelationship)
                return BadRequest("You already have an active caregiver relationship with this user");

            // Check if pending request already exists
            var existingRequest = await _context.CaregiverRequests
                .FirstOrDefaultAsync(cgr =>
                    cgr.SenderId == userId &&
                    cgr.RecipientId == dto.RecipientId &&
                    cgr.Status == CaregiverRequestStatus.Pending);

            if (existingRequest != null)
                return BadRequest("You already have a pending caregiver request with this user");

            var caregiverRequest = new CaregiverRequest
            {
                Id = Guid.NewGuid(),
                SenderId = userId,
                RecipientId = dto.RecipientId,
                Status = CaregiverRequestStatus.Pending,
                RequestMessage = dto.RequestMessage,
                RequestCanManageEvents = dto.RequestCanManageEvents,
                RequestCanManageProfile = dto.RequestCanManageProfile,
                RequestCanManageFriendships = dto.RequestCanManageFriendships,
                CreatedAt = DateTime.UtcNow
            };

            _context.CaregiverRequests.Add(caregiverRequest);
            await _context.SaveChangesAsync();

            var result = await _context.CaregiverRequests
                .Where(cgr => cgr.Id == caregiverRequest.Id)
                .Select(cgr => new CaregiverRequestDto
                {
                    Id = cgr.Id,
                    SenderId = cgr.SenderId,
                    SenderUsername = cgr.Sender.UserName ?? "",
                    SenderDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cgr.SenderId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    RecipientId = cgr.RecipientId,
                    RecipientUsername = cgr.Recipient.UserName ?? "",
                    RecipientDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cgr.RecipientId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    Status = cgr.Status.ToString(),
                    RequestMessage = cgr.RequestMessage,
                    RequestCanManageEvents = cgr.RequestCanManageEvents,
                    RequestCanManageProfile = cgr.RequestCanManageProfile,
                    RequestCanManageFriendships = cgr.RequestCanManageFriendships,
                    CreatedAt = cgr.CreatedAt,
                    RespondedAt = cgr.RespondedAt
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetReceivedRequests), result);
        }

        [HttpGet("received")]
        public async Task<ActionResult<IEnumerable<CaregiverRequestDto>>> GetReceivedRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var requests = await _context.CaregiverRequests
                .Where(cgr => cgr.RecipientId == userId && cgr.Status == CaregiverRequestStatus.Pending)
                .Select(cgr => new CaregiverRequestDto
                {
                    Id = cgr.Id,
                    SenderId = cgr.SenderId,
                    SenderUsername = cgr.Sender.UserName ?? "",
                    SenderDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cgr.SenderId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    RecipientId = cgr.RecipientId,
                    RecipientUsername = cgr.Recipient.UserName ?? "",
                    RecipientDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cgr.RecipientId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    Status = cgr.Status.ToString(),
                    RequestMessage = cgr.RequestMessage,
                    RequestCanManageEvents = cgr.RequestCanManageEvents,
                    RequestCanManageProfile = cgr.RequestCanManageProfile,
                    RequestCanManageFriendships = cgr.RequestCanManageFriendships,
                    CreatedAt = cgr.CreatedAt,
                    RespondedAt = cgr.RespondedAt
                })
                .OrderByDescending(cgr => cgr.CreatedAt)
                .ToListAsync();

            return Ok(requests);
        }

        [HttpGet("sent")]
        public async Task<ActionResult<IEnumerable<CaregiverRequestDto>>> GetSentRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var requests = await _context.CaregiverRequests
                .Where(cgr => cgr.SenderId == userId && cgr.Status == CaregiverRequestStatus.Pending)
                .Select(cgr => new CaregiverRequestDto
                {
                    Id = cgr.Id,
                    SenderId = cgr.SenderId,
                    SenderUsername = cgr.Sender.UserName ?? "",
                    SenderDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cgr.SenderId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    RecipientId = cgr.RecipientId,
                    RecipientUsername = cgr.Recipient.UserName ?? "",
                    RecipientDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cgr.RecipientId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    Status = cgr.Status.ToString(),
                    RequestMessage = cgr.RequestMessage,
                    RequestCanManageEvents = cgr.RequestCanManageEvents,
                    RequestCanManageProfile = cgr.RequestCanManageProfile,
                    RequestCanManageFriendships = cgr.RequestCanManageFriendships,
                    CreatedAt = cgr.CreatedAt,
                    RespondedAt = cgr.RespondedAt
                })
                .OrderByDescending(cgr => cgr.CreatedAt)
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPost("{requestId}/accept")]
        public async Task<ActionResult<CaregiverRequestDto>> AcceptCaregiverRequest(Guid requestId, [FromBody] AcceptCaregiverRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var request = await _context.CaregiverRequests
                .FirstOrDefaultAsync(cgr => cgr.Id == requestId);

            if (request == null)
                return NotFound("Caregiver request not found");

            if (request.RecipientId != userId)
                return Forbid();

            if (request.Status != CaregiverRequestStatus.Pending)
                return BadRequest("This request has already been responded to");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Update request status
                request.Status = CaregiverRequestStatus.Accepted;
                request.RespondedAt = DateTime.UtcNow;

                // Create CareRelationship with permissions (use overrides if provided, otherwise use requested permissions)
                var careRelationship = new CareRelationship
                {
                    Id = Guid.NewGuid(),
                    CaregiverId = request.SenderId,
                    RecipientId = request.RecipientId,
                    CanManageEvents = dto.CanManageEvents ?? request.RequestCanManageEvents,
                    CanManageProfile = dto.CanManageProfile ?? request.RequestCanManageProfile,
                    CanManageFriendships = dto.CanManageFriendships ?? request.RequestCanManageFriendships,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.CareRelationships.Add(careRelationship);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = await _context.CaregiverRequests
                    .Where(cgr => cgr.Id == requestId)
                    .Select(cgr => new CaregiverRequestDto
                    {
                        Id = cgr.Id,
                        SenderId = cgr.SenderId,
                        SenderUsername = cgr.Sender.UserName ?? "",
                        SenderDisplayName = _context.UserProfiles
                            .Where(up => up.UserId == cgr.SenderId)
                            .Select(up => up.DisplayName)
                            .FirstOrDefault(),
                        RecipientId = cgr.RecipientId,
                        RecipientUsername = cgr.Recipient.UserName ?? "",
                        RecipientDisplayName = _context.UserProfiles
                            .Where(up => up.UserId == cgr.RecipientId)
                            .Select(up => up.DisplayName)
                            .FirstOrDefault(),
                        Status = cgr.Status.ToString(),
                        RequestMessage = cgr.RequestMessage,
                        RequestCanManageEvents = cgr.RequestCanManageEvents,
                        RequestCanManageProfile = cgr.RequestCanManageProfile,
                        RequestCanManageFriendships = cgr.RequestCanManageFriendships,
                        CreatedAt = cgr.CreatedAt,
                        RespondedAt = cgr.RespondedAt
                    })
                    .FirstOrDefaultAsync();

                return Ok(result);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpPost("{requestId}/reject")]
        public async Task<ActionResult<CaregiverRequestDto>> RejectCaregiverRequest(Guid requestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var request = await _context.CaregiverRequests
                .FirstOrDefaultAsync(cgr => cgr.Id == requestId);

            if (request == null)
                return NotFound("Caregiver request not found");

            if (request.RecipientId != userId)
                return Forbid();

            if (request.Status != CaregiverRequestStatus.Pending)
                return BadRequest("This request has already been responded to");

            request.Status = CaregiverRequestStatus.Rejected;
            request.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = await _context.CaregiverRequests
                .Where(cgr => cgr.Id == requestId)
                .Select(cgr => new CaregiverRequestDto
                {
                    Id = cgr.Id,
                    SenderId = cgr.SenderId,
                    SenderUsername = cgr.Sender.UserName ?? "",
                    SenderDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cgr.SenderId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    RecipientId = cgr.RecipientId,
                    RecipientUsername = cgr.Recipient.UserName ?? "",
                    RecipientDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cgr.RecipientId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    Status = cgr.Status.ToString(),
                    RequestMessage = cgr.RequestMessage,
                    RequestCanManageEvents = cgr.RequestCanManageEvents,
                    RequestCanManageProfile = cgr.RequestCanManageProfile,
                    RequestCanManageFriendships = cgr.RequestCanManageFriendships,
                    CreatedAt = cgr.CreatedAt,
                    RespondedAt = cgr.RespondedAt
                })
                .FirstOrDefaultAsync();

            return Ok(result);
        }

        [HttpDelete("{requestId}")]
        public async Task<IActionResult> CancelCaregiverRequest(Guid requestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var request = await _context.CaregiverRequests
                .FirstOrDefaultAsync(cgr => cgr.Id == requestId);

            if (request == null)
                return NotFound("Caregiver request not found");

            if (request.SenderId != userId)
                return Forbid();

            if (request.Status != CaregiverRequestStatus.Pending)
                return BadRequest("Can only cancel pending requests");

            _context.CaregiverRequests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("status/{otherUserId}")]
        public async Task<ActionResult<object>> GetCaregiverRequestStatus(string otherUserId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Check if active relationship exists (I'm caregiver for them)
            var activeRelationship = await _context.CareRelationships
                .FirstOrDefaultAsync(cr =>
                    cr.CaregiverId == userId &&
                    cr.RecipientId == otherUserId &&
                    cr.IsActive);

            if (activeRelationship != null)
            {
                return Ok(new
                {
                    Status = "ActiveCaregiver",
                    RelationshipId = activeRelationship.Id,
                    CanManageEvents = activeRelationship.CanManageEvents,
                    CanManageProfile = activeRelationship.CanManageProfile,
                    CanManageFriendships = activeRelationship.CanManageFriendships
                });
            }

            // Check for pending request (I sent to them)
            var pendingRequest = await _context.CaregiverRequests
                .FirstOrDefaultAsync(cgr =>
                    cgr.SenderId == userId &&
                    cgr.RecipientId == otherUserId &&
                    cgr.Status == CaregiverRequestStatus.Pending);

            if (pendingRequest != null)
            {
                return Ok(new
                {
                    Status = "PendingSent",
                    RequestId = pendingRequest.Id
                });
            }

            return Ok(new { Status = "None" });
        }
    }
}
