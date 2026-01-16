using Diversion.Models;
using Diversion.Constants;
using Diversion.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Diversion.Helpers;

public static class NotificationHelper
{
    public static async Task CreateNotificationAsync(
        DiversionDbContext context,
        string userId,
        string type,
        string message,
        string? referenceId = null,
        string? actionUrl = null,
        IHubContext<NotificationHub>? hubContext = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Message = message,
            ReferenceId = referenceId,
            ActionUrl = actionUrl,
            CreatedAt = DateTime.UtcNow
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        // Push real-time notification via SignalR if hub context is available
        if (hubContext != null)
        {
            await hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", new
            {
                id = notification.Id,
                type = notification.Type,
                message = notification.Message,
                referenceId = notification.ReferenceId,
                actionUrl = notification.ActionUrl,
                createdAt = notification.CreatedAt,
                isRead = false
            });
        }
    }

    // Convenience methods for common notification types
    public static async Task NotifyFriendRequestAsync(
        DiversionDbContext context,
        string recipientId,
        string senderUsername,
        string requestId,
        IHubContext<NotificationHub>? hubContext = null)
    {
        await CreateNotificationAsync(
            context,
            recipientId,
            NotificationTypeConstants.FriendRequest,
            $"{senderUsername} sent you a friend request",
            requestId,
            "/friends/requests",
            hubContext);
    }

    public static async Task NotifyFriendRequestAcceptedAsync(
        DiversionDbContext context,
        string senderId,
        string accepterUsername,
        IHubContext<NotificationHub>? hubContext = null)
    {
        await CreateNotificationAsync(
            context,
            senderId,
            NotificationTypeConstants.FriendRequestAccepted,
            $"{accepterUsername} accepted your friend request",
            null,
            "/friends",
            hubContext);
    }

    public static async Task NotifyEventRSVPAsync(
        DiversionDbContext context,
        string organizerId,
        string attendeeUsername,
        string eventId,
        string eventTitle,
        IHubContext<NotificationHub>? hubContext = null)
    {
        await CreateNotificationAsync(
            context,
            organizerId,
            NotificationTypeConstants.EventRSVP,
            $"{attendeeUsername} is attending \"{eventTitle}\"",
            eventId,
            $"/events/{eventId}",
            hubContext);
    }

    public static async Task NotifyNewMessageAsync(
        DiversionDbContext context,
        string receiverId,
        string senderUsername,
        IHubContext<NotificationHub>? hubContext = null)
    {
        await CreateNotificationAsync(
            context,
            receiverId,
            NotificationTypeConstants.NewMessage,
            $"New message from {senderUsername}",
            null,
            "/messages",
            hubContext);
    }

    public static async Task NotifyCaregiverRequestAsync(
        DiversionDbContext context,
        string recipientId,
        string senderUsername,
        string requestId,
        IHubContext<NotificationHub>? hubContext = null)
    {
        await CreateNotificationAsync(
            context,
            recipientId,
            NotificationTypeConstants.CaregiverRequest,
            $"{senderUsername} sent you a caregiver request",
            requestId,
            "/caregiver/requests",
            hubContext);
    }

    public static async Task NotifyCaregiverRequestAcceptedAsync(
        DiversionDbContext context,
        string senderId,
        string accepterUsername,
        IHubContext<NotificationHub>? hubContext = null)
    {
        await CreateNotificationAsync(
            context,
            senderId,
            NotificationTypeConstants.CaregiverRequestAccepted,
            $"{accepterUsername} accepted your caregiver request",
            null,
            "/caregiver/recipients",
            hubContext);
    }
}
