using Microsoft.EntityFrameworkCore;

namespace Diversion.Helpers;

/// <summary>
/// Helper class for filtering out blocked and banned users from queries
/// </summary>
public static class UserFilterHelper
{
    /// <summary>
    /// Gets the list of user IDs that should be excluded from results for the given user.
    /// This includes users who have blocked or been blocked by the user, plus banned users.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="userId">The current user's ID</param>
    /// <returns>A HashSet of user IDs to exclude</returns>
    public static async Task<HashSet<string>> GetExcludedUserIdsAsync(
        DiversionDbContext context,
        string userId)
    {
        var blockedUserIds = await GetBlockedUserIdsAsync(context, userId);
        var bannedUserIds = await GetBannedUserIdsAsync(context);

        return blockedUserIds.Union(bannedUserIds).ToHashSet();
    }

    /// <summary>
    /// Gets the list of user IDs that the current user has blocked or been blocked by
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="userId">The current user's ID</param>
    /// <returns>A list of blocked user IDs</returns>
    public static async Task<List<string>> GetBlockedUserIdsAsync(
        DiversionDbContext context,
        string userId)
    {
        return await context.UserBlocks
            .AsNoTracking()
            .Where(ub => ub.BlockerId == userId || ub.BlockedUserId == userId)
            .Select(ub => ub.BlockerId == userId ? ub.BlockedUserId : ub.BlockerId)
            .Distinct()
            .ToListAsync();
    }

    /// <summary>
    /// Gets the list of all banned user IDs
    /// </summary>
    /// <param name="context">The database context</param>
    /// <returns>A list of banned user IDs</returns>
    public static async Task<List<string>> GetBannedUserIdsAsync(DiversionDbContext context)
    {
        return await context.UserProfiles
            .AsNoTracking()
            .Where(up => up.IsBanned)
            .Select(up => up.UserId)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if two users are blocked (in either direction)
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="userId1">First user ID</param>
    /// <param name="userId2">Second user ID</param>
    /// <returns>True if either user has blocked the other</returns>
    public static async Task<bool> AreUsersBlockedAsync(
        DiversionDbContext context,
        string userId1,
        string userId2)
    {
        return await context.UserBlocks
            .AsNoTracking()
            .AnyAsync(ub =>
                (ub.BlockerId == userId1 && ub.BlockedUserId == userId2) ||
                (ub.BlockerId == userId2 && ub.BlockedUserId == userId1));
    }
}
