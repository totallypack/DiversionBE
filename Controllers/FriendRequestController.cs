using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Diversion.DTOs;
using Diversion.Models;
using Diversion.Helpers;

namespace Diversion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FriendRequestController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        [HttpPost("send")]
        public async Task<ActionResult<FriendRequestDto>> SendFriendRequest([FromBody] SendFriendRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Validate caregiver action if acting on behalf
            var authResult = await CaregiverAuthHelper.ValidateAndAuthorize(
                _context, userId, dto.ActingOnBehalfOf, CaregiverPermission.ManageFriendships);

            if (!authResult.IsAuthorized)
                return BadRequest(authResult.ErrorMessage);

            // Use effective user ID (recipient if caregiver, otherwise self)
            var effectiveUserId = authResult.EffectiveUserId;

            if (effectiveUserId == dto.ReceiverId)
                return BadRequest("You cannot send a friend request to yourself");

            var receiverExists = await _context.Users.AnyAsync(u => u.Id == dto.ReceiverId);
            if (!receiverExists)
                return BadRequest("User not found");

            // Check if already friends
            var alreadyFriends = await _context.Friendships
                .AnyAsync(f => f.UserId == effectiveUserId && f.FriendId == dto.ReceiverId);

            if (alreadyFriends)
                return BadRequest("Already friends with this user");

            // Check if pending request already exists
            var existingRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr =>
                    (fr.SenderId == effectiveUserId && fr.ReceiverId == dto.ReceiverId && fr.Status == FriendRequestStatus.Pending) ||
                    (fr.SenderId == dto.ReceiverId && fr.ReceiverId == effectiveUserId && fr.Status == FriendRequestStatus.Pending));

            if (existingRequest != null)
            {
                if (existingRequest.SenderId == effectiveUserId)
                    return BadRequest("You already have a pending friend request with this user");
                else
                    return BadRequest("This user has already sent you a friend request. Check your received requests.");
            }

            var friendRequest = new FriendRequest
            {
                Id = Guid.NewGuid(),
                SenderId = effectiveUserId,
                ReceiverId = dto.ReceiverId,
                Status = FriendRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.FriendRequests.Add(friendRequest);
            await _context.SaveChangesAsync();

            // Create notification for recipient
            var senderUser = await _context.Users.FindAsync(effectiveUserId);
            await NotificationHelper.NotifyFriendRequestAsync(
                _context,
                dto.ReceiverId,
                senderUser?.UserName ?? "Someone",
                friendRequest.Id.ToString());

            var result = await _context.FriendRequests
                .Where(fr => fr.Id == friendRequest.Id)
                .Select(fr => new FriendRequestDto
                {
                    Id = fr.Id,
                    SenderId = fr.SenderId,
                    SenderUsername = fr.Sender.UserName ?? "",
                    SenderDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == fr.SenderId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    ReceiverId = fr.ReceiverId,
                    ReceiverUsername = fr.Receiver.UserName ?? "",
                    ReceiverDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == fr.ReceiverId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    Status = fr.Status.ToString(),
                    CreatedAt = fr.CreatedAt,
                    RespondedAt = fr.RespondedAt
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetReceivedRequests), result);
        }

        [HttpGet("received")]
        public async Task<ActionResult<IEnumerable<FriendRequestDto>>> GetReceivedRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var requests = await _context.FriendRequests
                .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending)
                .Select(fr => new FriendRequestDto
                {
                    Id = fr.Id,
                    SenderId = fr.SenderId,
                    SenderUsername = fr.Sender.UserName ?? "",
                    SenderDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == fr.SenderId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    ReceiverId = fr.ReceiverId,
                    ReceiverUsername = fr.Receiver.UserName ?? "",
                    ReceiverDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == fr.ReceiverId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    Status = fr.Status.ToString(),
                    CreatedAt = fr.CreatedAt,
                    RespondedAt = fr.RespondedAt
                })
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();

            return Ok(requests);
        }

        [HttpGet("sent")]
        public async Task<ActionResult<IEnumerable<FriendRequestDto>>> GetSentRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var requests = await _context.FriendRequests
                .Where(fr => fr.SenderId == userId && fr.Status == FriendRequestStatus.Pending)
                .Select(fr => new FriendRequestDto
                {
                    Id = fr.Id,
                    SenderId = fr.SenderId,
                    SenderUsername = fr.Sender.UserName ?? "",
                    SenderDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == fr.SenderId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    ReceiverId = fr.ReceiverId,
                    ReceiverUsername = fr.Receiver.UserName ?? "",
                    ReceiverDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == fr.ReceiverId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    Status = fr.Status.ToString(),
                    CreatedAt = fr.CreatedAt,
                    RespondedAt = fr.RespondedAt
                })
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPost("{requestId}/accept")]
        public async Task<ActionResult<FriendRequestDto>> AcceptFriendRequest(Guid requestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var friendRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.Id == requestId);

            if (friendRequest == null)
                return NotFound("Friend request not found");

            if (friendRequest.ReceiverId != userId)
                return Forbid();

            if (friendRequest.Status != FriendRequestStatus.Pending)
                return BadRequest("This request has already been responded to");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Update friend request status
                friendRequest.Status = FriendRequestStatus.Accepted;
                friendRequest.RespondedAt = DateTime.UtcNow;

                // Create bidirectional friendships
                var friendship1 = new Friendship
                {
                    Id = Guid.NewGuid(),
                    UserId = friendRequest.SenderId,
                    FriendId = friendRequest.ReceiverId,
                    CreatedAt = DateTime.UtcNow
                };

                var friendship2 = new Friendship
                {
                    Id = Guid.NewGuid(),
                    UserId = friendRequest.ReceiverId,
                    FriendId = friendRequest.SenderId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Friendships.Add(friendship1);
                _context.Friendships.Add(friendship2);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Create notification for original sender
                var currentUser = await _context.Users.FindAsync(userId);
                await NotificationHelper.NotifyFriendRequestAcceptedAsync(
                    _context,
                    friendRequest.SenderId,
                    currentUser?.UserName ?? "Someone");

                var result = new FriendRequestDto
                {
                    Id = friendRequest.Id,
                    SenderId = friendRequest.SenderId,
                    SenderUsername = (await _context.Users.FindAsync(friendRequest.SenderId))?.UserName ?? "",
                    SenderDisplayName = await _context.UserProfiles
                        .Where(up => up.UserId == friendRequest.SenderId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefaultAsync(),
                    ReceiverId = friendRequest.ReceiverId,
                    ReceiverUsername = (await _context.Users.FindAsync(friendRequest.ReceiverId))?.UserName ?? "",
                    ReceiverDisplayName = await _context.UserProfiles
                        .Where(up => up.UserId == friendRequest.ReceiverId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefaultAsync(),
                    Status = friendRequest.Status.ToString(),
                    CreatedAt = friendRequest.CreatedAt,
                    RespondedAt = friendRequest.RespondedAt
                };

                return Ok(result);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpPost("{requestId}/reject")]
        public async Task<ActionResult<FriendRequestDto>> RejectFriendRequest(Guid requestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var friendRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.Id == requestId);

            if (friendRequest == null)
                return NotFound("Friend request not found");

            if (friendRequest.ReceiverId != userId)
                return Forbid();

            if (friendRequest.Status != FriendRequestStatus.Pending)
                return BadRequest("This request has already been responded to");

            friendRequest.Status = FriendRequestStatus.Rejected;
            friendRequest.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new FriendRequestDto
            {
                Id = friendRequest.Id,
                SenderId = friendRequest.SenderId,
                SenderUsername = (await _context.Users.FindAsync(friendRequest.SenderId))?.UserName ?? "",
                SenderDisplayName = await _context.UserProfiles
                    .Where(up => up.UserId == friendRequest.SenderId)
                    .Select(up => up.DisplayName)
                    .FirstOrDefaultAsync(),
                ReceiverId = friendRequest.ReceiverId,
                ReceiverUsername = (await _context.Users.FindAsync(friendRequest.ReceiverId))?.UserName ?? "",
                ReceiverDisplayName = await _context.UserProfiles
                    .Where(up => up.UserId == friendRequest.ReceiverId)
                    .Select(up => up.DisplayName)
                    .FirstOrDefaultAsync(),
                Status = friendRequest.Status.ToString(),
                CreatedAt = friendRequest.CreatedAt,
                RespondedAt = friendRequest.RespondedAt
            };

            return Ok(result);
        }

        [HttpDelete("{requestId}")]
        public async Task<IActionResult> CancelFriendRequest(Guid requestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var friendRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.Id == requestId);

            if (friendRequest == null)
                return NotFound("Friend request not found");

            if (friendRequest.SenderId != userId)
                return Forbid();

            if (friendRequest.Status != FriendRequestStatus.Pending)
                return BadRequest("Can only cancel pending requests");

            _context.FriendRequests.Remove(friendRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("status/{otherUserId}")]
        public async Task<ActionResult<object>> GetFriendRequestStatus(string otherUserId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Check if already friends
            var areFriends = await _context.Friendships
                .AnyAsync(f => f.UserId == userId && f.FriendId == otherUserId);

            if (areFriends)
            {
                return Ok(new { status = "friends", requestId = (Guid?)null });
            }

            // Check for pending requests
            var pendingRequest = await _context.FriendRequests
                .Where(fr =>
                    ((fr.SenderId == userId && fr.ReceiverId == otherUserId) ||
                     (fr.SenderId == otherUserId && fr.ReceiverId == userId)) &&
                    fr.Status == FriendRequestStatus.Pending)
                .Select(fr => new { fr.Id, fr.SenderId })
                .FirstOrDefaultAsync();

            if (pendingRequest != null)
            {
                var requestStatus = pendingRequest.SenderId == userId ? "sent" : "received";
                return Ok(new { status = requestStatus, requestId = pendingRequest.Id });
            }

            return Ok(new { status = "none", requestId = (Guid?)null });
        }
    }
}
