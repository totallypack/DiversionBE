using Diversion.DTOs;
using Diversion.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Diversion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserProfileController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        [HttpGet("me")]
        public async Task<ActionResult<UserProfileWithInterestsDto>> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var profile = await _context.UserProfiles
                .Include(up => up.UserInterests)
                    .ThenInclude(ui => ui.SubInterest)
                        .ThenInclude(si => si.Interest)
                .Where(up => up.UserId == userId)
                .Select(up => new UserProfileWithInterestsDto
                {
                    Id = up.Id,
                    UserId = up.UserId,
                    DisplayName = up.DisplayName,
                    Bio = up.Bio,
                    Location = up.Location,
                    DOB = up.DOB,
                    ProfilePicUrl = up.ProfilePicUrl,
                    Interests = up.UserInterests.Select(ui => new SubInterestDto
                    {
                        Id = ui.SubInterest.Id,
                        Name = ui.SubInterest.Name,
                        InterestId = ui.SubInterest.Interest.Id,
                        Description = ui.SubInterest.Description,
                        IconUrl = ui.SubInterest.IconUrl
                    }).ToList()
                }).FirstOrDefaultAsync();
            if (profile == null)
                return NotFound();
            return Ok(profile);
        }

        [HttpPost]
        public async Task<ActionResult<UserProfileDto>> CreateProfile([FromBody] CreateUserProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var existingProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (existingProfile != null)
                return BadRequest("User profile already exists");

            var profile = new UserProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DisplayName = dto.DisplayName,
                Bio = dto.Bio,
                Location = dto.Location,
                DOB = dto.DOB,
                ProfilePicUrl = dto.ProfilePicUrl
            };

            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();

            var result = new UserProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                DisplayName = profile.DisplayName,
                Bio = profile.Bio,
                Location = profile.Location,
                DOB = profile.DOB,
                ProfilePicUrl = profile.ProfilePicUrl
            };

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (profile == null)
                return NotFound();

            if (dto.DisplayName != null)
                profile.DisplayName = dto.DisplayName;
            if (dto.Bio != null)
                profile.Bio = dto.Bio;
            if (dto.Location != null)
                profile.Location = dto.Location;
            if (dto.DOB.HasValue)
                profile.DOB = dto.DOB;
            if (dto.ProfilePicUrl != null)
                profile.ProfilePicUrl = dto.ProfilePicUrl;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (profile == null)
                return NotFound();

            _context.UserProfiles.Remove(profile);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
