using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Diversion.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time messaging (direct messages and community chat)
    /// </summary>
    [Authorize]
    public class MessageHub(DiversionDbContext context) : Hub
    {
        private readonly DiversionDbContext _context = context;
        /// <summary>
        /// Called when a client connects to the hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to their personal group for direct messages
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects from the hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join a community chat room (validates membership first)
        /// </summary>
        /// <param name="communityId">The community ID to join</param>
        public async Task JoinCommunity(string communityId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(communityId, out var communityGuid))
                return;

            // Validate user is a member of the community
            var isMember = await _context.CommunityMemberships
                .AsNoTracking()
                .AnyAsync(cm => cm.CommunityId == communityGuid && cm.UserId == userId);

            if (isMember)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"community_{communityId}");
            }
        }

        /// <summary>
        /// Leave a community chat room
        /// </summary>
        /// <param name="communityId">The community ID to leave</param>
        public async Task LeaveCommunity(string communityId)
        {
            // No validation needed for leaving - just remove from group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"community_{communityId}");
        }

        /// <summary>
        /// Notify other users that current user is typing in a direct message conversation
        /// </summary>
        /// <param name="receiverId">The user ID of the message recipient</param>
        public async Task SendTypingIndicator(string receiverId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Clients.Group($"user_{receiverId}").SendAsync("UserTyping", userId);
            }
        }

        /// <summary>
        /// Notify other users that current user is typing in a community chat (validates membership)
        /// </summary>
        /// <param name="communityId">The community ID</param>
        public async Task SendCommunityTypingIndicator(string communityId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(communityId, out var communityGuid))
                return;

            // Validate user is a member before sending typing indicator
            var isMember = await _context.CommunityMemberships
                .AsNoTracking()
                .AnyAsync(cm => cm.CommunityId == communityGuid && cm.UserId == userId);

            if (isMember)
            {
                await Clients.OthersInGroup($"community_{communityId}").SendAsync("UserTypingInCommunity", userId, communityId);
            }
        }
    }
}
