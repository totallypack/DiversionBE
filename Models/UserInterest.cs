using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Diversion.Models
{
    public class UserInterest
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; }
        [Required]
        public Guid SubInterestId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public IdentityUser User { get; set; }
        public SubInterest SubInterest { get; set; }
    }
}
