using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Diversion.Models
{
    public class FriendRequest
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }

        // Navigation properties
        public IdentityUser Sender { get; set; }
        public IdentityUser Receiver { get; set; }
    }
}
