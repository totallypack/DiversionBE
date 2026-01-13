using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Diversion.DTOs;
using Diversion.Models;

namespace Diversion.Controllers
{
    [ApiController]
    [Route("api/communities/{communityId}/messages")]
    [Authorize]
    public class CommunityMessagesController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommunityMessageDto>>> GetMessages(
            Guid communityId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Verify user is a member
            var isMember = await _context.CommunityMemberships
                .AnyAsync(cm => cm.CommunityId == communityId && cm.UserId == userId);

            if (!isMember)
                return Forbid();

            if (take > 100)
                take = 100;

            var messages = await _context.CommunityMessages
                .Where(cm => cm.CommunityId == communityId)
                .OrderByDescending(cm => cm.SentAt)
                .Skip(skip)
                .Take(take)
                .Select(cm => new CommunityMessageDto
                {
                    Id = cm.Id,
                    CommunityId = cm.CommunityId,
                    SenderId = cm.SenderId,
                    SenderUsername = cm.Sender.UserName ?? "",
                    SenderDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cm.SenderId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    Content = cm.Content,
                    SentAt = cm.SentAt,
                    ReplyToMessageId = cm.ReplyToMessageId,
                    ReplyToSenderName = cm.ReplyToMessage != null
                        ? _context.UserProfiles
                            .Where(up => up.UserId == cm.ReplyToMessage.SenderId)
                            .Select(up => up.DisplayName)
                            .FirstOrDefault() ?? cm.ReplyToMessage.Sender.UserName
                        : null
                })
                .ToListAsync();

            // Reverse to get chronological order
            messages.Reverse();

            return Ok(messages);
        }

        [HttpPost]
        public async Task<ActionResult<CommunityMessageDto>> SendMessage(
            Guid communityId,
            [FromBody] CreateCommunityMessageDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Verify user is a member
            var isMember = await _context.CommunityMemberships
                .AnyAsync(cm => cm.CommunityId == communityId && cm.UserId == userId);

            if (!isMember)
                return Forbid();

            // Verify reply-to message exists if provided
            if (dto.ReplyToMessageId.HasValue)
            {
                var replyToExists = await _context.CommunityMessages
                    .AnyAsync(cm => cm.Id == dto.ReplyToMessageId.Value && cm.CommunityId == communityId);

                if (!replyToExists)
                    return BadRequest("Reply-to message not found");
            }

            var message = new CommunityMessage
            {
                Id = Guid.NewGuid(),
                CommunityId = communityId,
                SenderId = userId,
                Content = dto.Content!,
                SentAt = DateTime.UtcNow,
                ReplyToMessageId = dto.ReplyToMessageId
            };

            _context.CommunityMessages.Add(message);
            await _context.SaveChangesAsync();

            var result = await _context.CommunityMessages
                .Where(cm => cm.Id == message.Id)
                .Select(cm => new CommunityMessageDto
                {
                    Id = cm.Id,
                    CommunityId = cm.CommunityId,
                    SenderId = cm.SenderId,
                    SenderUsername = cm.Sender.UserName ?? "",
                    SenderDisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cm.SenderId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault(),
                    Content = cm.Content,
                    SentAt = cm.SentAt,
                    ReplyToMessageId = cm.ReplyToMessageId,
                    ReplyToSenderName = cm.ReplyToMessage != null
                        ? _context.UserProfiles
                            .Where(up => up.UserId == cm.ReplyToMessage.SenderId)
                            .Select(up => up.DisplayName)
                            .FirstOrDefault() ?? cm.ReplyToMessage.Sender.UserName
                        : null
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetMessages), new { communityId }, result);
        }
    }
}
