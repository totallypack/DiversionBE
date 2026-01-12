using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Diversion.Models
{
    public class UserProfile
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ProfilePicUrl { get; set; }
        public DateTime? DOB {  get; set; }

        // User Type
        public UserType UserType { get; set; } = UserType.Regular;

        // Business-specific fields
        public string? BusinessName { get; set; }
        public string? BusinessWebsite { get; set; }
        public string? BusinessHours { get; set; }
        public string? BusinessCategory { get; set; }
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }

        public IdentityUser? User { get; set; }
        public ICollection<UserInterest> UserInterests { get; set; }
    }
}
