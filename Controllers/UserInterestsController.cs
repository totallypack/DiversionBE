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
    public class UserInterestsController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserInterestWithDetailsDto>>> GetMyInterests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var interests = await _context.UserInterests
                .Where(ui => ui.UserId == userId)
                .Select(ui => new UserInterestWithDetailsDto
                {
                    Id = ui.Id,
                    CreatedAt = ui.CreatedAt,
                    SubInterest = new SubInterestWithInterestDto
                    {
                        Id = ui.SubInterest.Id,
                        Name = ui.SubInterest.Name,
                        Description = ui.SubInterest.Description,
                        IconUrl = ui.SubInterest.IconUrl,
                        Interest = new InterestDto
                        {
                            Id = ui.SubInterest.Interest.Id,
                            Name = ui.SubInterest.Interest.Name,
                            Description = ui.SubInterest.Interest.Description,
                            IconUrl = ui.SubInterest.Interest.IconUrl
                        }
                    }
                })
                .ToListAsync();

            return Ok(interests);
        }

        [HttpPost]
        public async Task<ActionResult<UserInterestDto>> AddInterest([FromBody] AddUserInterestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var subInterestExists = await _context.SubInterests
                .AnyAsync(si => si.Id == dto.SubInterestId);

            if (!subInterestExists)
                return BadRequest("SubInterest not found");

            var existingInterest = await _context.UserInterests
                .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.SubInterestId == dto.SubInterestId);

            if (existingInterest != null)
                return BadRequest("Interest already added");

            var userInterest = new UserInterest
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SubInterestId = dto.SubInterestId,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserInterests.Add(userInterest);
            await _context.SaveChangesAsync();

            var result = new UserInterestDto
            {
                Id = userInterest.Id,
                UserId = userInterest.UserId,
                SubInterestId = userInterest.SubInterestId,
                CreatedAt = userInterest.CreatedAt
            };

            return CreatedAtAction(nameof(GetMyInterests), result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveInterest(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var userInterest = await _context.UserInterests
                .FirstOrDefaultAsync(ui => ui.Id == id && ui.UserId == userId);

            if (userInterest == null)
                return NotFound();

            _context.UserInterests.Remove(userInterest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("subinterest/{subInterestId}")]
        public async Task<IActionResult> RemoveInterestBySubInterestId(Guid subInterestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var userInterest = await _context.UserInterests
                .FirstOrDefaultAsync(ui => ui.SubInterestId == subInterestId && ui.UserId == userId);

            if (userInterest == null)
                return NotFound();

            _context.UserInterests.Remove(userInterest);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}