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
    public class UserBlocksController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserBlockDto>>> GetBlockedUsers()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var blockedUsers = await _context.UserBlocks
                .AsNoTracking()
                .Where(ub => ub.BlockerId == userId)
                .Select(ub => new UserBlockDto
                {
                    Id = ub.Id,
                    BlockedUserId = ub.BlockedUserId,
                    BlockedUsername = ub.BlockedUser.UserName ?? "",
                    BlockedDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == ub.BlockedUserId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    CreatedAt = ub.CreatedAt
                })
                .OrderByDescending(ub => ub.CreatedAt)
                .ToListAsync();

            return Ok(blockedUsers);
        }

        [HttpPost]
        public async Task<ActionResult<UserBlockDto>> BlockUser([FromBody] CreateUserBlockDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (userId == dto.BlockedUserId)
                return BadRequest("Cannot block yourself");

            var userToBlock = await _context.Users.FindAsync(dto.BlockedUserId);
            if (userToBlock == null)
                return BadRequest("User not found");

            // Check if already blocked
            var existingBlock = await _context.UserBlocks
                .FirstOrDefaultAsync(ub => ub.BlockerId == userId && ub.BlockedUserId == dto.BlockedUserId);

            if (existingBlock != null)
                return BadRequest("User is already blocked");

            var userBlock = new UserBlock
            {
                BlockerId = userId,
                BlockedUserId = dto.BlockedUserId,
                Reason = dto.Reason,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserBlocks.Add(userBlock);
            await _context.SaveChangesAsync();

            var result = await _context.UserBlocks
                .AsNoTracking()
                .Where(ub => ub.Id == userBlock.Id)
                .Select(ub => new UserBlockDto
                {
                    Id = ub.Id,
                    BlockedUserId = ub.BlockedUserId,
                    BlockedUsername = ub.BlockedUser.UserName ?? "",
                    BlockedDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == ub.BlockedUserId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    CreatedAt = ub.CreatedAt
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetBlockedUsers), result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> UnblockUser(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var userBlock = await _context.UserBlocks
                .FirstOrDefaultAsync(ub => ub.Id == id && ub.BlockerId == userId);

            if (userBlock == null)
                return NotFound();

            _context.UserBlocks.Remove(userBlock);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("status/{otherUserId}")]
        public async Task<ActionResult<object>> GetBlockingStatus(string otherUserId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Check if current user blocked the other user
            var userBlockedOther = await _context.UserBlocks
                .AsNoTracking()
                .AnyAsync(ub => ub.BlockerId == userId && ub.BlockedUserId == otherUserId);

            // Check if other user blocked current user
            var otherBlockedUser = await _context.UserBlocks
                .AsNoTracking()
                .AnyAsync(ub => ub.BlockerId == otherUserId && ub.BlockedUserId == userId);

            return Ok(new
            {
                isBlocked = userBlockedOther || otherBlockedUser,
                userBlockedOther = userBlockedOther,
                otherBlockedUser = otherBlockedUser
            });
        }

        // Helper method for other controllers to check blocking status
        public static async Task<bool> AreUsersBlockedAsync(DiversionDbContext context, string userId1, string userId2)
        {
            return await context.UserBlocks
                .AsNoTracking()
                .AnyAsync(ub =>
                    (ub.BlockerId == userId1 && ub.BlockedUserId == userId2) ||
                    (ub.BlockerId == userId2 && ub.BlockedUserId == userId1));
        }
    }
}
