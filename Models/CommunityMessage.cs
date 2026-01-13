using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Diversion.Models
{
    public class CommunityMessage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CommunityId { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        [MaxLength(5000)]
        public string Content { get; set; }

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public Guid? ReplyToMessageId { get; set; }

        // Navigation properties
        public Community Community { get; set; }
        public IdentityUser Sender { get; set; }
        public CommunityMessage? ReplyToMessage { get; set; }
    }
}
