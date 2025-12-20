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
    public class FriendshipsController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<FriendshipDto>>> GetMyFriends()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var friendships = await _context.Friendships
                .Where(f => f.UserId == userId)
                .Select(f => new FriendshipDto
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    FriendId = f.FriendId,
                    FriendUsername = f.Friend.UserName ?? "",
                    FriendDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == f.FriendId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    CreatedAt = f.CreatedAt
                })
                .OrderBy(f => f.FriendDisplayName)
                .ToListAsync();

            return Ok(friendships);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<UserSearchDto>>> SearchUsers([FromQuery] string query)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return BadRequest("Search query must be at least 2 characters");

            var myFriendIds = await _context.Friendships
                .Where(f => f.UserId == userId)
                .Select(f => f.FriendId)
                .ToListAsync();

            var users = await _context.Users
                .Where(u => u.UserName != null && u.UserName.Contains(query) && u.Id != userId)
                .Take(20)
                .Select(u => new UserSearchDto
                {
                    UserId = u.Id,
                    Username = u.UserName ?? "",
                    DisplayName = _context.UserProfiles
                        .Where(up => up.UserId == u.Id)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    City = _context.UserProfiles
                        .Where(up => up.UserId == u.Id)
                        .Select(up => up.City)
                        .FirstOrDefault(),
                    State = _context.UserProfiles
                        .Where(up => up.UserId == u.Id)
                        .Select(up => up.State)
                        .FirstOrDefault(),
                    IsFriend = myFriendIds.Contains(u.Id)
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost]
        public async Task<ActionResult<FriendshipDto>> AddFriend([FromBody] CreateFriendshipDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (userId == dto.FriendId)
                return BadRequest("You cannot add yourself as a friend");

            var friendExists = await _context.Users.AnyAsync(u => u.Id == dto.FriendId);
            if (!friendExists)
                return BadRequest("User not found");

            var existingFriendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == userId && f.FriendId == dto.FriendId);

            if (existingFriendship != null)
                return BadRequest("Already friends with this user");

            var friendship1 = new Friendship
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FriendId = dto.FriendId,
                CreatedAt = DateTime.UtcNow
            };

            var friendship2 = new Friendship
            {
                Id = Guid.NewGuid(),
                UserId = dto.FriendId,
                FriendId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Friendships.Add(friendship1);
            _context.Friendships.Add(friendship2);
            await _context.SaveChangesAsync();

            var result = await _context.Friendships
                .Where(f => f.Id == friendship1.Id)
                .Select(f => new FriendshipDto
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    FriendId = f.FriendId,
                    FriendUsername = f.Friend.UserName ?? "",
                    FriendDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == f.FriendId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    CreatedAt = f.CreatedAt
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetMyFriends), result);
        }

        [HttpDelete("{friendId}")]
        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var friendship1 = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == userId && f.FriendId == friendId);

            var friendship2 = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == friendId && f.FriendId == userId);

            if (friendship1 == null && friendship2 == null)
                return NotFound("Friendship not found");

            if (friendship1 != null)
                _context.Friendships.Remove(friendship1);

            if (friendship2 != null)
                _context.Friendships.Remove(friendship2);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("check/{otherUserId}")]
        public async Task<ActionResult<bool>> CheckFriendship(string otherUserId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var areFriends = await _context.Friendships
                .AnyAsync(f => f.UserId == userId && f.FriendId == otherUserId);

            return Ok(areFriends);
        }
    }
}
