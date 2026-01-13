using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Diversion.Models
{
    public class DirectMessage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; }

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        // Navigation properties
        public IdentityUser Sender { get; set; }
        public IdentityUser Receiver { get; set; }
    }
}
