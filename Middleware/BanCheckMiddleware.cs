using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Diversion.Middleware
{
    public class BanCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public BanCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, DiversionDbContext dbContext)
        {
            // Skip middleware for non-API routes
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }

            // Skip middleware for auth endpoints
            if (context.Request.Path.StartsWithSegments("/api/auth"))
            {
                await _next(context);
                return;
            }

            // Only check if user is authenticated
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(userId))
                {
                    // Check if user is banned
                    var userProfile = await dbContext.UserProfiles
                        .AsNoTracking()
                        .FirstOrDefaultAsync(up => up.UserId == userId);

                    if (userProfile?.IsBanned == true)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = "Your account has been banned.",
                            reason = userProfile.BanReason ?? "Violation of community guidelines",
                            bannedAt = userProfile.BannedAt
                        });
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
