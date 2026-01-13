using System.ComponentModel.DataAnnotations;

namespace Diversion.DTOs
{
    public class CommunityMessageDto
    {
        public Guid Id { get; set; }
        public Guid CommunityId { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderUsername { get; set; } = string.Empty;
        public string? SenderDisplayName { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public Guid? ReplyToMessageId { get; set; }
        public string? ReplyToSenderName { get; set; }
    }

    public class CreateCommunityMessageDto
    {
        [Required]
        [StringLength(5000, MinimumLength = 1)]
        public string? Content { get; set; }

        public Guid? ReplyToMessageId { get; set; }
    }

    public class DirectMessageDto
    {
        public Guid Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderUsername { get; set; } = string.Empty;
        public string? SenderDisplayName { get; set; }
        public string ReceiverId { get; set; } = string.Empty;
        public string ReceiverUsername { get; set; } = string.Empty;
        public string? ReceiverDisplayName { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class SendDirectMessageDto
    {
        [Required]
        public string? ReceiverId { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string? Content { get; set; }
    }

    public class ConversationDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? LastMessageContent { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
    }
}
