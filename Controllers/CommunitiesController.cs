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
    public class CommunitiesController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        [HttpGet("public")]
        public async Task<ActionResult<IEnumerable<CommunityDto>>> GetPublicCommunities()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var communities = await _context.Communities
                .Where(c => !c.IsPrivate)
                .Select(c => new CommunityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    CreatorId = c.CreatorId,
                    CreatorUsername = c.Creator.UserName ?? "",
                    CreatorDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == c.CreatorId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    InterestId = c.InterestId,
                    InterestName = c.Interest != null ? c.Interest.Name : null,
                    ImageUrl = c.ImageUrl,
                    IsPrivate = c.IsPrivate,
                    CreatedAt = c.CreatedAt,
                    MemberCount = c.Members.Count(),
                    IsMember = c.Members.Any(m => m.UserId == userId),
                    UserRole = c.Members.Where(m => m.UserId == userId).Select(m => m.Role).FirstOrDefault()
                })
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(communities);
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<CommunityDto>>> GetMyCommunities()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var communities = await _context.CommunityMemberships
                .Where(cm => cm.UserId == userId)
                .Select(cm => new CommunityDto
                {
                    Id = cm.Community.Id,
                    Name = cm.Community.Name,
                    Description = cm.Community.Description,
                    CreatorId = cm.Community.CreatorId,
                    CreatorUsername = cm.Community.Creator.UserName ?? "",
                    CreatorDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cm.Community.CreatorId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    InterestId = cm.Community.InterestId,
                    InterestName = cm.Community.Interest != null ? cm.Community.Interest.Name : null,
                    ImageUrl = cm.Community.ImageUrl,
                    IsPrivate = cm.Community.IsPrivate,
                    CreatedAt = cm.Community.CreatedAt,
                    MemberCount = cm.Community.Members.Count(),
                    IsMember = true,
                    UserRole = cm.Role
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(communities);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommunityDto>> GetCommunity(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var community = await _context.Communities
                .Where(c => c.Id == id)
                .Select(c => new CommunityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    CreatorId = c.CreatorId,
                    CreatorUsername = c.Creator.UserName ?? "",
                    CreatorDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == c.CreatorId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    InterestId = c.InterestId,
                    InterestName = c.Interest != null ? c.Interest.Name : null,
                    ImageUrl = c.ImageUrl,
                    IsPrivate = c.IsPrivate,
                    CreatedAt = c.CreatedAt,
                    MemberCount = c.Members.Count(),
                    IsMember = c.Members.Any(m => m.UserId == userId),
                    UserRole = c.Members.Where(m => m.UserId == userId).Select(m => m.Role).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (community == null)
                return NotFound();

            return Ok(community);
        }

        [HttpPost]
        public async Task<ActionResult<CommunityDto>> CreateCommunity([FromBody] CreateCommunityDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (dto.InterestId.HasValue)
            {
                var interestExists = await _context.Interests.AnyAsync(i => i.Id == dto.InterestId.Value);
                if (!interestExists)
                    return BadRequest("Invalid interest ID");
            }

            var community = new Community
            {
                Id = Guid.NewGuid(),
                Name = dto.Name!,
                Description = dto.Description,
                CreatorId = userId,
                InterestId = dto.InterestId,
                ImageUrl = dto.ImageUrl,
                IsPrivate = dto.IsPrivate,
                CreatedAt = DateTime.UtcNow
            };

            _context.Communities.Add(community);

            // Add creator as owner
            var membership = new CommunityMembership
            {
                Id = Guid.NewGuid(),
                CommunityId = community.Id,
                UserId = userId,
                Role = "Owner",
                JoinedAt = DateTime.UtcNow
            };

            _context.CommunityMemberships.Add(membership);
            await _context.SaveChangesAsync();

            var result = await GetCommunity(community.Id);
            return CreatedAtAction(nameof(GetCommunity), new { id = community.Id }, result.Value);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCommunity(Guid id, [FromBody] UpdateCommunityDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var community = await _context.Communities
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (community == null)
                return NotFound();

            var membership = community.Members.FirstOrDefault(m => m.UserId == userId);
            if (membership == null || (membership.Role != "Owner" && membership.Role != "Moderator"))
                return Forbid();

            community.Name = dto.Name!;
            community.Description = dto.Description;
            community.ImageUrl = dto.ImageUrl;
            community.IsPrivate = dto.IsPrivate;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCommunity(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var community = await _context.Communities
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (community == null)
                return NotFound();

            var membership = community.Members.FirstOrDefault(m => m.UserId == userId);
            if (membership == null || membership.Role != "Owner")
                return Forbid();

            _context.Communities.Remove(community);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/join")]
        public async Task<IActionResult> JoinCommunity(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var community = await _context.Communities
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (community == null)
                return NotFound();

            if (community.IsPrivate)
                return BadRequest("Cannot join private communities without an invitation");

            if (community.Members.Any(m => m.UserId == userId))
                return BadRequest("Already a member of this community");

            var membership = new CommunityMembership
            {
                Id = Guid.NewGuid(),
                CommunityId = id,
                UserId = userId,
                Role = "Member",
                JoinedAt = DateTime.UtcNow
            };

            _context.CommunityMemberships.Add(membership);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{id}/leave")]
        public async Task<IActionResult> LeaveCommunity(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var membership = await _context.CommunityMemberships
                .FirstOrDefaultAsync(cm => cm.CommunityId == id && cm.UserId == userId);

            if (membership == null)
                return NotFound("Not a member of this community");

            if (membership.Role == "Owner")
                return BadRequest("Owners cannot leave their community. Delete the community instead.");

            _context.CommunityMemberships.Remove(membership);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/members")]
        public async Task<ActionResult<IEnumerable<CommunityMembershipDto>>> GetMembers(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var community = await _context.Communities
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (community == null)
                return NotFound();

            var isMember = community.Members.Any(m => m.UserId == userId);
            if (community.IsPrivate && !isMember)
                return Forbid();

            var members = await _context.CommunityMemberships
                .Where(cm => cm.CommunityId == id)
                .Select(cm => new CommunityMembershipDto
                {
                    Id = cm.Id,
                    CommunityId = cm.CommunityId,
                    UserId = cm.UserId,
                    Username = cm.User.UserName ?? "",
                    DisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cm.UserId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    Role = cm.Role,
                    JoinedAt = cm.JoinedAt
                })
                .OrderBy(cm => cm.Role == "Owner" ? 0 : cm.Role == "Moderator" ? 1 : 2)
                .ThenBy(cm => cm.JoinedAt)
                .ToListAsync();

            return Ok(members);
        }
    }
}
