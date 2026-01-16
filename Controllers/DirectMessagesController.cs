using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Diversion.DTOs;
using Diversion.Hubs;
using Diversion.Models;
using Diversion.Helpers;

namespace Diversion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DirectMessagesController(
        DiversionDbContext context,
        IHubContext<NotificationHub> notificationHub,
        IHubContext<MessageHub> messageHub) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;
        private readonly IHubContext<NotificationHub> _notificationHub = notificationHub;
        private readonly IHubContext<MessageHub> _messageHub = messageHub;

        [HttpGet("conversations")]
        public async Task<ActionResult<IEnumerable<ConversationDto>>> GetConversations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Get blocked user IDs
            var blockedUserIds = await UserFilterHelper.GetBlockedUserIdsAsync(_context, userId);

            // Get all unique users that current user has messaged with
            var sentTo = await _context.DirectMessages
                .AsNoTracking()
                .Where(dm => dm.SenderId == userId)
                .Select(dm => dm.ReceiverId)
                .Distinct()
                .ToListAsync();

            var receivedFrom = await _context.DirectMessages
                .AsNoTracking()
                .Where(dm => dm.ReceiverId == userId)
                .Select(dm => dm.SenderId)
                .Distinct()
                .ToListAsync();

            var allUserIds = sentTo.Union(receivedFrom).Distinct()
                .Where(id => !blockedUserIds.Contains(id))
                .ToList();

            var conversations = new List<ConversationDto>();

            foreach (var otherUserId in allUserIds)
            {
                var lastMessage = await _context.DirectMessages
                    .AsNoTracking()
                    .Where(dm =>
                        (dm.SenderId == userId && dm.ReceiverId == otherUserId) ||
                        (dm.SenderId == otherUserId && dm.ReceiverId == userId))
                    .OrderByDescending(dm => dm.SentAt)
                    .FirstOrDefaultAsync();

                var unreadCount = await _context.DirectMessages
                    .AsNoTracking()
                    .Where(dm => dm.SenderId == otherUserId && dm.ReceiverId == userId && !dm.IsRead)
                    .CountAsync();

                var otherUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == otherUserId);
                var otherProfile = await _context.UserProfiles
                    .AsNoTracking()
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

            // Check if users are blocked
            var areBlocked = await _context.UserBlocks
                .AsNoTracking()
                .AnyAsync(ub =>
                    (ub.BlockerId == userId && ub.BlockedUserId == otherUserId) ||
                    (ub.BlockerId == otherUserId && ub.BlockedUserId == userId));

            if (areBlocked)
                return Forbid();

            // Verify users are friends
            var areFriends = await _context.Friendships
                .AsNoTracking()
                .AnyAsync(f => f.UserId == userId && f.FriendId == otherUserId);

            if (!areFriends)
                return Forbid();

            if (take > 100)
                take = 100;

            var messages = await _context.DirectMessages
                .AsNoTracking()
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

            // Check if users are blocked
            var areBlocked = await _context.UserBlocks
                .AsNoTracking()
                .AnyAsync(ub =>
                    (ub.BlockerId == userId && ub.BlockedUserId == dto.ReceiverId) ||
                    (ub.BlockerId == dto.ReceiverId && ub.BlockedUserId == userId));

            if (areBlocked)
                return BadRequest("Cannot send message to this user");

            // Verify users are friends
            var areFriends = await _context.Friendships
                .AsNoTracking()
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

            // Create notification for receiver (with real-time push)
            var sender = await _context.Users.FindAsync(userId);
            await NotificationHelper.NotifyNewMessageAsync(
                _context,
                dto.ReceiverId!,
                sender?.UserName ?? "Someone",
                _notificationHub);

            // Get sender profile for display name
            var senderProfile = await _context.UserProfiles
                .AsNoTracking()
                .Where(up => up.UserId == userId)
                .Select(up => up.DisplayName)
                .FirstOrDefaultAsync();

            // Push real-time message to receiver via SignalR
            await _messageHub.Clients.Group($"user_{dto.ReceiverId}").SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                senderId = message.SenderId,
                senderUsername = sender?.UserName ?? "",
                senderDisplayName = senderProfile,
                receiverId = message.ReceiverId,
                content = message.Content,
                sentAt = message.SentAt,
                isRead = false
            });

            var result = await _context.DirectMessages
                .AsNoTracking()
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

            // Get blocked user IDs
            var blockedUserIds = await UserFilterHelper.GetBlockedUserIdsAsync(_context, userId);

            var count = await _context.DirectMessages
                .AsNoTracking()
                .Where(dm => dm.ReceiverId == userId &&
                           !dm.IsRead &&
                           !blockedUserIds.Contains(dm.SenderId))
                .CountAsync();

            return Ok(count);
        }
    }
}
