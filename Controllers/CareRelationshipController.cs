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
    public class CareRelationshipController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        [HttpGet("my-caregivers")]
        public async Task<ActionResult<IEnumerable<CareRelationshipDto>>> GetMyCaregivers()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var caregivers = await _context.CareRelationships
                .Where(cr => cr.RecipientId == userId && cr.IsActive)
                .Select(cr => new CareRelationshipDto
                {
                    Id = cr.Id,
                    CaregiverId = cr.CaregiverId,
                    CaregiverUsername = cr.Caregiver.UserName ?? "",
                    CaregiverDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cr.CaregiverId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    RecipientId = cr.RecipientId,
                    RecipientUsername = cr.Recipient.UserName ?? "",
                    RecipientDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cr.RecipientId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    CanManageEvents = cr.CanManageEvents,
                    CanManageProfile = cr.CanManageProfile,
                    CanManageFriendships = cr.CanManageFriendships,
                    CreatedAt = cr.CreatedAt,
                    RevokedAt = cr.RevokedAt,
                    IsActive = cr.IsActive
                })
                .ToListAsync();

            return Ok(caregivers);
        }

        [HttpGet("my-recipients")]
        public async Task<ActionResult<IEnumerable<CareRelationshipDto>>> GetMyRecipients()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Verify user has Caregiver user type
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userProfile == null || userProfile.UserType != UserType.Caregiver)
                return BadRequest("Only users with Caregiver user type can access recipient information");

            var recipients = await _context.CareRelationships
                .Where(cr => cr.CaregiverId == userId && cr.IsActive)
                .Select(cr => new CareRelationshipDto
                {
                    Id = cr.Id,
                    CaregiverId = cr.CaregiverId,
                    CaregiverUsername = cr.Caregiver.UserName ?? "",
                    CaregiverDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cr.CaregiverId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    RecipientId = cr.RecipientId,
                    RecipientUsername = cr.Recipient.UserName ?? "",
                    RecipientDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cr.RecipientId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    CanManageEvents = cr.CanManageEvents,
                    CanManageProfile = cr.CanManageProfile,
                    CanManageFriendships = cr.CanManageFriendships,
                    CreatedAt = cr.CreatedAt,
                    RevokedAt = cr.RevokedAt,
                    IsActive = cr.IsActive
                })
                .ToListAsync();

            return Ok(recipients);
        }

        [HttpGet("{relationshipId}")]
        public async Task<ActionResult<CareRelationshipDto>> GetCareRelationship(Guid relationshipId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var relationship = await _context.CareRelationships
                .Where(cr => cr.Id == relationshipId)
                .Select(cr => new CareRelationshipDto
                {
                    Id = cr.Id,
                    CaregiverId = cr.CaregiverId,
                    CaregiverUsername = cr.Caregiver.UserName ?? "",
                    CaregiverDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cr.CaregiverId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    RecipientId = cr.RecipientId,
                    RecipientUsername = cr.Recipient.UserName ?? "",
                    RecipientDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cr.RecipientId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    CanManageEvents = cr.CanManageEvents,
                    CanManageProfile = cr.CanManageProfile,
                    CanManageFriendships = cr.CanManageFriendships,
                    CreatedAt = cr.CreatedAt,
                    RevokedAt = cr.RevokedAt,
                    IsActive = cr.IsActive
                })
                .FirstOrDefaultAsync();

            if (relationship == null)
                return NotFound("Care relationship not found");

            // Verify user is either caregiver or recipient
            if (relationship.CaregiverId != userId && relationship.RecipientId != userId)
                return Forbid();

            return Ok(relationship);
        }

        [HttpPut("{relationshipId}/permissions")]
        public async Task<IActionResult> UpdatePermissions(Guid relationshipId, [FromBody] UpdateCareRelationshipPermissionsDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var relationship = await _context.CareRelationships
                .FirstOrDefaultAsync(cr => cr.Id == relationshipId);

            if (relationship == null)
                return NotFound("Care relationship not found");

            // Only recipient can update permissions
            if (relationship.RecipientId != userId)
                return Forbid();

            if (!relationship.IsActive)
                return BadRequest("Cannot update permissions for inactive relationship");

            // Update only provided permissions
            if (dto.CanManageEvents.HasValue)
                relationship.CanManageEvents = dto.CanManageEvents.Value;

            if (dto.CanManageProfile.HasValue)
                relationship.CanManageProfile = dto.CanManageProfile.Value;

            if (dto.CanManageFriendships.HasValue)
                relationship.CanManageFriendships = dto.CanManageFriendships.Value;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{relationshipId}/revoke")]
        public async Task<IActionResult> RevokeCareRelationship(Guid relationshipId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var relationship = await _context.CareRelationships
                .FirstOrDefaultAsync(cr => cr.Id == relationshipId);

            if (relationship == null)
                return NotFound("Care relationship not found");

            // Only recipient can revoke
            if (relationship.RecipientId != userId)
                return Forbid();

            if (!relationship.IsActive)
                return BadRequest("Relationship is already inactive");

            relationship.IsActive = false;
            relationship.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Caregiver access revoked", RevokedAt = relationship.RevokedAt });
        }

        [HttpPost("{relationshipId}/reactivate")]
        public async Task<IActionResult> ReactivateCareRelationship(Guid relationshipId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var relationship = await _context.CareRelationships
                .FirstOrDefaultAsync(cr => cr.Id == relationshipId);

            if (relationship == null)
                return NotFound("Care relationship not found");

            // Only recipient can reactivate
            if (relationship.RecipientId != userId)
                return Forbid();

            if (relationship.IsActive)
                return BadRequest("Relationship is already active");

            relationship.IsActive = true;
            relationship.RevokedAt = null;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Caregiver access reactivated" });
        }

        [HttpDelete("{relationshipId}")]
        public async Task<IActionResult> DeleteCareRelationship(Guid relationshipId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var relationship = await _context.CareRelationships
                .FirstOrDefaultAsync(cr => cr.Id == relationshipId);

            if (relationship == null)
                return NotFound("Care relationship not found");

            // Either party can delete permanently
            if (relationship.CaregiverId != userId && relationship.RecipientId != userId)
                return Forbid();

            _context.CareRelationships.Remove(relationship);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("check/{recipientId}")]
        public async Task<ActionResult<object>> CheckCareRelationship(string recipientId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var relationship = await _context.CareRelationships
                .FirstOrDefaultAsync(cr =>
                    cr.CaregiverId == userId &&
                    cr.RecipientId == recipientId &&
                    cr.IsActive);

            if (relationship == null)
                return Ok(new { IsCaregiver = false });

            return Ok(new
            {
                IsCaregiver = true,
                RelationshipId = relationship.Id,
                CanManageEvents = relationship.CanManageEvents,
                CanManageProfile = relationship.CanManageProfile,
                CanManageFriendships = relationship.CanManageFriendships
            });
        }
    }
}
