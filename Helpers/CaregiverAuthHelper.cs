using Microsoft.EntityFrameworkCore;

namespace Diversion.Helpers
{
    public static class CaregiverAuthHelper
    {
        public static async Task<CaregiverAuthResult> ValidateAndAuthorize(
            DiversionDbContext context,
            string requestingUserId,
            string? actingOnBehalfOf,
            CaregiverPermission requiredPermission)
        {
            if (string.IsNullOrEmpty(actingOnBehalfOf))
                return CaregiverAuthResult.Success(requestingUserId);

            var relationship = await context.CareRelationships
                .FirstOrDefaultAsync(cr =>
                    cr.CaregiverId == requestingUserId &&
                    cr.RecipientId == actingOnBehalfOf &&
                    cr.IsActive);

            if (relationship == null)
                return CaregiverAuthResult.Failure("No active caregiver relationship");

            bool hasPermission = requiredPermission switch
            {
                CaregiverPermission.ManageEvents => relationship.CanManageEvents,
                CaregiverPermission.ManageProfile => relationship.CanManageProfile,
                CaregiverPermission.ManageFriendships => relationship.CanManageFriendships,
                _ => false
            };

            return hasPermission ?
                CaregiverAuthResult.Success(actingOnBehalfOf) :
                CaregiverAuthResult.Failure("Insufficient permissions");
        }
    }

    public enum CaregiverPermission
    {
        ManageEvents,
        ManageProfile,
        ManageFriendships
    }

    public class CaregiverAuthResult
    {
        public bool IsAuthorized { get; set; }
        public string? ErrorMessage { get; set; }
        public string EffectiveUserId { get; set; }  // ID to use for operations

        public static CaregiverAuthResult Success(string effectiveUserId) =>
            new() { IsAuthorized = true, EffectiveUserId = effectiveUserId };

        public static CaregiverAuthResult Failure(string error) =>
            new() { IsAuthorized = false, ErrorMessage = error, EffectiveUserId = string.Empty };
    }
}
