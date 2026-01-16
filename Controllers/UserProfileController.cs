using Diversion.DTOs;
using Diversion.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Diversion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserProfileController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        /// <summary>
        /// Retrieves the authenticated user's own profile
        /// </summary>
        /// <returns>Current user's profile including interests and account-type-specific fields</returns>
        /// <response code="200">Returns the user's profile</response>
        /// <response code="401">Unauthorized - authentication required</response>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserProfileWithInterestsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserProfileWithInterestsDto>> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var profile = await _context.UserProfiles
                .AsNoTracking()
                .Where(up => up.UserId == userId)
                .Select(up => new UserProfileWithInterestsDto
                {
                    Id = up.Id,
                    UserId = up.UserId,
                    DisplayName = up.DisplayName,
                    Bio = up.Bio,
                    City = up.City,
                    State = up.State,
                    DOB = up.DOB,
                    ProfilePicUrl = up.ProfilePicUrl,
                    UserType = up.UserType.ToString(),
                    BusinessName = up.BusinessName,
                    BusinessWebsite = up.BusinessWebsite,
                    BusinessHours = up.BusinessHours,
                    BusinessCategory = up.BusinessCategory,
                    IsVerified = up.IsVerified,
                    VerifiedAt = up.VerifiedAt,
                    CaregiverCredentials = up.CaregiverCredentials,
                    YearsOfExperience = up.YearsOfExperience,
                    Certifications = up.Certifications,
                    CareTypes = up.CareTypes,
                    Specializations = up.Specializations,
                    IsBackgroundChecked = up.IsBackgroundChecked,
                    LicenseNumber = up.LicenseNumber,
                    LicenseExpiry = up.LicenseExpiry,
                    EmploymentStatus = up.EmploymentStatus,
                    Interests = up.UserInterests.Select(ui => new SubInterestWithInterestDto
                    {
                        Id = ui.SubInterest.Id,
                        Name = ui.SubInterest.Name,
                        Description = ui.SubInterest.Description,
                        IconUrl = ui.SubInterest.IconUrl,
                        Interest = new InterestDto
                        {
                            Id = ui.SubInterest.Interest.Id,
                            Name = ui.SubInterest.Interest.Name,
                            Description = ui.SubInterest.Interest.Description
                        }
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (profile == null)
            {
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                return Ok(new UserProfileWithInterestsDto
                {
                    Id = Guid.Empty,
                    UserId = userId,
                    DisplayName = user?.UserName ?? "Unknown User",
                    Bio = null,
                    City = null,
                    State = null,
                    DOB = null,
                    ProfilePicUrl = null,
                    Interests = new List<SubInterestWithInterestDto>()
                });
            }

            return Ok(profile);
        }

        /// <summary>
        /// Retrieves a specific user's profile by user ID
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>User profile including interests, bio, location, and account type</returns>
        /// <response code="200">Returns the user profile</response>
        /// <response code="404">User not found</response>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(UserProfileWithInterestsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileWithInterestsDto>> GetUserProfile(string userId)
        {
            var userExists = await _context.Users.AsNoTracking().AnyAsync(u => u.Id == userId);
            if (!userExists)
                return NotFound(new { message = "User not found" });

            var profile = await _context.UserProfiles
                .AsNoTracking()
                .Where(up => up.UserId == userId)
                .Select(up => new UserProfileWithInterestsDto
                {
                    Id = up.Id,
                    UserId = up.UserId,
                    DisplayName = up.DisplayName,
                    Bio = up.Bio,
                    City = up.City,
                    State = up.State,
                    DOB = up.DOB,
                    ProfilePicUrl = up.ProfilePicUrl,
                    UserType = up.UserType.ToString(),
                    BusinessName = up.BusinessName,
                    BusinessWebsite = up.BusinessWebsite,
                    BusinessHours = up.BusinessHours,
                    BusinessCategory = up.BusinessCategory,
                    IsVerified = up.IsVerified,
                    VerifiedAt = up.VerifiedAt,
                    CaregiverCredentials = up.CaregiverCredentials,
                    YearsOfExperience = up.YearsOfExperience,
                    Certifications = up.Certifications,
                    CareTypes = up.CareTypes,
                    Specializations = up.Specializations,
                    IsBackgroundChecked = up.IsBackgroundChecked,
                    LicenseNumber = up.LicenseNumber,
                    LicenseExpiry = up.LicenseExpiry,
                    EmploymentStatus = up.EmploymentStatus,
                    Interests = _context.UserInterests
                        .Where(ui => ui.UserId == userId)
                        .Select(ui => new SubInterestWithInterestDto
                        {
                            Id = ui.SubInterest.Id,
                            Name = ui.SubInterest.Name,
                            Description = ui.SubInterest.Description,
                            IconUrl = ui.SubInterest.IconUrl,
                            Interest = new InterestDto
                            {
                                Id = ui.SubInterest.Interest.Id,
                                Name = ui.SubInterest.Interest.Name,
                                Description = ui.SubInterest.Interest.Description
                            }
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (profile == null)
            {
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                return Ok(new UserProfileWithInterestsDto
                {
                    Id = Guid.Empty,
                    UserId = userId,
                    DisplayName = user?.UserName ?? "Unknown User",
                    Bio = null,
                    City = null,
                    State = null,
                    DOB = null,
                    ProfilePicUrl = null,
                    Interests = new List<SubInterestWithInterestDto>()
                });
            }

            return Ok(profile);
        }

        [HttpPost]
        public async Task<ActionResult<UserProfileDto>> CreateProfile([FromBody] CreateUserProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var existingProfile = await _context.UserProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (existingProfile != null)
                return BadRequest("User profile already exists");

            // Parse UserType from string, default to Regular if invalid
            UserType userType = UserType.Regular;
            if (!string.IsNullOrEmpty(dto.UserType) && Enum.TryParse<UserType>(dto.UserType, out var parsedType))
            {
                userType = parsedType;
            }

            var profile = new UserProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DisplayName = dto.DisplayName,
                Bio = dto.Bio,
                City = dto.City,
                State = dto.State,
                DOB = dto.DOB,
                ProfilePicUrl = dto.ProfilePicUrl,
                UserType = userType,
                BusinessName = dto.BusinessName,
                BusinessWebsite = dto.BusinessWebsite,
                BusinessHours = dto.BusinessHours,
                BusinessCategory = dto.BusinessCategory,
                CaregiverCredentials = dto.CaregiverCredentials,
                YearsOfExperience = dto.YearsOfExperience,
                Certifications = dto.Certifications,
                CareTypes = dto.CareTypes,
                Specializations = dto.Specializations,
                LicenseNumber = dto.LicenseNumber,
                LicenseExpiry = dto.LicenseExpiry,
                EmploymentStatus = dto.EmploymentStatus
            };

            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();

            var result = new UserProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                DisplayName = profile.DisplayName,
                Bio = profile.Bio,
                City = profile.City,
                State = profile.State,
                DOB = profile.DOB,
                ProfilePicUrl = profile.ProfilePicUrl,
                UserType = profile.UserType.ToString(),
                BusinessName = profile.BusinessName,
                BusinessWebsite = profile.BusinessWebsite,
                BusinessHours = profile.BusinessHours,
                BusinessCategory = profile.BusinessCategory,
                IsVerified = profile.IsVerified,
                VerifiedAt = profile.VerifiedAt,
                CaregiverCredentials = profile.CaregiverCredentials,
                YearsOfExperience = profile.YearsOfExperience,
                Certifications = profile.Certifications,
                CareTypes = profile.CareTypes,
                Specializations = profile.Specializations,
                IsBackgroundChecked = profile.IsBackgroundChecked,
                LicenseNumber = profile.LicenseNumber,
                LicenseExpiry = profile.LicenseExpiry,
                EmploymentStatus = profile.EmploymentStatus
            };

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (profile == null)
                return NotFound();

            if (dto.DisplayName != null)
                profile.DisplayName = dto.DisplayName;
            if (dto.Bio != null)
                profile.Bio = dto.Bio;
            if (dto.City != null)
                profile.City = dto.City;
            if (dto.State != null)
                profile.State = dto.State;
            if (dto.DOB.HasValue)
                profile.DOB = dto.DOB;
            if (dto.ProfilePicUrl != null)
                profile.ProfilePicUrl = dto.ProfilePicUrl;
            if (dto.UserType != null && Enum.TryParse<UserType>(dto.UserType, out var parsedType))
                profile.UserType = parsedType;
            if (dto.BusinessName != null)
                profile.BusinessName = dto.BusinessName;
            if (dto.BusinessWebsite != null)
                profile.BusinessWebsite = dto.BusinessWebsite;
            if (dto.BusinessHours != null)
                profile.BusinessHours = dto.BusinessHours;
            if (dto.BusinessCategory != null)
                profile.BusinessCategory = dto.BusinessCategory;

            // Caregiver fields
            if (dto.CaregiverCredentials != null)
                profile.CaregiverCredentials = dto.CaregiverCredentials;
            if (dto.YearsOfExperience.HasValue)
                profile.YearsOfExperience = dto.YearsOfExperience;
            if (dto.Certifications != null)
                profile.Certifications = dto.Certifications;
            if (dto.CareTypes != null)
                profile.CareTypes = dto.CareTypes;
            if (dto.Specializations != null)
                profile.Specializations = dto.Specializations;
            if (dto.LicenseNumber != null)
                profile.LicenseNumber = dto.LicenseNumber;
            if (dto.LicenseExpiry.HasValue)
                profile.LicenseExpiry = dto.LicenseExpiry;
            if (dto.EmploymentStatus != null)
                profile.EmploymentStatus = dto.EmploymentStatus;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (profile == null)
                return NotFound();

            _context.UserProfiles.Remove(profile);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
