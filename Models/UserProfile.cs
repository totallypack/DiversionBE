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
        public string? Location { get; set; }
        public string? ProfilePicUrl { get; set; }
        public DateTime? DOB {  get; set; }
        public IdentityUser? User { get; set; }
        public ICollection<UserInterest> UserInterests { get; set; }
    }
}
