using Diversion.Models;
using Diversion.Constants;

namespace Diversion.Helpers;

public static class NotificationHelper
{
    public static async Task CreateNotificationAsync(
        DiversionDbContext context,
        string userId,
        string type,
        string message,
        string? referenceId = null,
        string? actionUrl = null)
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
    }

    // Convenience methods for common notification types
    public static async Task NotifyFriendRequestAsync(
        DiversionDbContext context,
        string recipientId,
        string senderUsername,
        string requestId)
    {
        await CreateNotificationAsync(
            context,
            recipientId,
            NotificationTypeConstants.FriendRequest,
            $"{senderUsername} sent you a friend request",
            requestId,
            "/friends/requests");
    }

    public static async Task NotifyFriendRequestAcceptedAsync(
        DiversionDbContext context,
        string senderId,
        string accepterUsername)
    {
        await CreateNotificationAsync(
            context,
            senderId,
            NotificationTypeConstants.FriendRequestAccepted,
            $"{accepterUsername} accepted your friend request",
            null,
            "/friends");
    }

    public static async Task NotifyEventRSVPAsync(
        DiversionDbContext context,
        string organizerId,
        string attendeeUsername,
        string eventId,
        string eventTitle)
    {
        await CreateNotificationAsync(
            context,
            organizerId,
            NotificationTypeConstants.EventRSVP,
            $"{attendeeUsername} is attending \"{eventTitle}\"",
            eventId,
            $"/events/{eventId}");
    }

    public static async Task NotifyNewMessageAsync(
        DiversionDbContext context,
        string receiverId,
        string senderUsername)
    {
        await CreateNotificationAsync(
            context,
            receiverId,
            NotificationTypeConstants.NewMessage,
            $"New message from {senderUsername}",
            null,
            "/messages");
    }

    public static async Task NotifyCaregiverRequestAsync(
        DiversionDbContext context,
        string recipientId,
        string senderUsername,
        string requestId)
    {
        await CreateNotificationAsync(
            context,
            recipientId,
            NotificationTypeConstants.CaregiverRequest,
            $"{senderUsername} sent you a caregiver request",
            requestId,
            "/caregiver/requests");
    }

    public static async Task NotifyCaregiverRequestAcceptedAsync(
        DiversionDbContext context,
        string senderId,
        string accepterUsername)
    {
        await CreateNotificationAsync(
            context,
            senderId,
            NotificationTypeConstants.CaregiverRequestAccepted,
            $"{accepterUsername} accepted your caregiver request",
            null,
            "/caregiver/recipients");
    }
}
