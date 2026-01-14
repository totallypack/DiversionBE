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
    public class DirectMessagesController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        [HttpGet("conversations")]
        public async Task<ActionResult<IEnumerable<ConversationDto>>> GetConversations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Get all unique users that current user has messaged with
            var sentTo = await _context.DirectMessages
                .Where(dm => dm.SenderId == userId)
                .Select(dm => dm.ReceiverId)
                .Distinct()
                .ToListAsync();

            var receivedFrom = await _context.DirectMessages
                .Where(dm => dm.ReceiverId == userId)
                .Select(dm => dm.SenderId)
                .Distinct()
                .ToListAsync();

            var allUserIds = sentTo.Union(receivedFrom).Distinct().ToList();

            var conversations = new List<ConversationDto>();

            foreach (var otherUserId in allUserIds)
            {
                var lastMessage = await _context.DirectMessages
                    .Where(dm =>
                        (dm.SenderId == userId && dm.ReceiverId == otherUserId) ||
                        (dm.SenderId == otherUserId && dm.ReceiverId == userId))
                    .OrderByDescending(dm => dm.SentAt)
                    .FirstOrDefaultAsync();

                var unreadCount = await _context.DirectMessages
                    .Where(dm => dm.SenderId == otherUserId && dm.ReceiverId == userId && !dm.IsRead)
                    .CountAsync();

                var otherUser = await _context.Users.FindAsync(otherUserId);
                var otherProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.UserId == otherUserId);

                if (lastMessage != null && otherUser != null)
                {
                    conversations.Add(new ConversationDto
                    {
                        UserId = otherUserId,
                        Username = otherUser.UserName ?? "",
                        DisplayName = otherProfile?.DisplayName,
                        LastMessageContent = lastMessage.Content,
                        LastMessageTime = lastMessage.SentAt,
                        UnreadCount = unreadCount
                    });
                }
            }

            conversations = conversations
                .OrderByDescending(c => c.LastMessageTime)
                .ToList();

            return Ok(conversations);
        }

        [HttpGet("with/{otherUserId}")]
        public async Task<ActionResult<IEnumerable<DirectMessageDto>>> GetMessagesWith(
            string otherUserId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Verify users are friends
            var areFriends = await _context.Friendships
                .AnyAsync(f => f.UserId == userId && f.FriendId == otherUserId);

            if (!areFriends)
                return Forbid();

            if (take > 100)
                take = 100;

            var messages = await _context.DirectMessages
                .Where(dm =>
                    (dm.SenderId == userId && dm.ReceiverId == otherUserId) ||
                    (dm.SenderId == otherUserId && dm.ReceiverId == userId))
                .OrderByDescending(dm => dm.SentAt)
                .Skip(skip)
                .Take(take)
                .Select(dm => new DirectMessageDto
                {
                    Id = dm.Id,
                    SenderId = dm.SenderId,
                    SenderUsername = dm.Sender.UserName ?? "",
                    SenderDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == dm.SenderId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    ReceiverId = dm.ReceiverId,
                    ReceiverUsername = dm.Receiver.UserName ?? "",
                    ReceiverDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == dm.ReceiverId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    Content = dm.Content,
                    SentAt = dm.SentAt,
                    IsRead = dm.IsRead,
                    ReadAt = dm.ReadAt
                })
                .ToListAsync();

            // Mark unread messages as read
            var unreadMessages = await _context.DirectMessages
                .Where(dm => dm.SenderId == otherUserId && dm.ReceiverId == userId && !dm.IsRead)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
            }

            if (unreadMessages.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            // Reverse to get chronological order
            messages.Reverse();

            return Ok(messages);
        }

        [HttpPost("send")]
        public async Task<ActionResult<DirectMessageDto>> SendMessage([FromBody] SendDirectMessageDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (userId == dto.ReceiverId)
                return BadRequest("Cannot send message to yourself");

            // Verify users are friends
            var areFriends = await _context.Friendships
                .AnyAsync(f => f.UserId == userId && f.FriendId == dto.ReceiverId);

            if (!areFriends)
                return BadRequest("Can only send messages to friends");

            var message = new DirectMessage
            {
                Id = Guid.NewGuid(),
                SenderId = userId,
                ReceiverId = dto.ReceiverId!,
                Content = dto.Content!,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.DirectMessages.Add(message);
            await _context.SaveChangesAsync();

            // Create notification for receiver
            var sender = await _context.Users.FindAsync(userId);
            await NotificationHelper.NotifyNewMessageAsync(
                _context,
                dto.ReceiverId!,
                sender?.UserName ?? "Someone");

            var result = await _context.DirectMessages
                .Where(dm => dm.Id == message.Id)
                .Select(dm => new DirectMessageDto
                {
                    Id = dm.Id,
                    SenderId = dm.SenderId,
                    SenderUsername = dm.Sender.UserName ?? "",
                    SenderDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == dm.SenderId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    ReceiverId = dm.ReceiverId,
                    ReceiverUsername = dm.Receiver.UserName ?? "",
                    ReceiverDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == dm.ReceiverId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    Content = dm.Content,
                    SentAt = dm.SentAt,
                    IsRead = dm.IsRead,
                    ReadAt = dm.ReadAt
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetMessagesWith), new { otherUserId = dto.ReceiverId }, result);
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var count = await _context.DirectMessages
                .Where(dm => dm.ReceiverId == userId && !dm.IsRead)
                .CountAsync();

            return Ok(count);
        }
    }
}
