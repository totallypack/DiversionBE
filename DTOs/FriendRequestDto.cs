using System.ComponentModel.DataAnnotations;

namespace Diversion.DTOs
{
    public class FriendRequestDto
    {
        public Guid Id { get; set; }
        public string? SenderId { get; set; }
        public string? SenderUsername { get; set; }
        public string? SenderDisplayName { get; set; }
        public string? ReceiverId { get; set; }
        public string? ReceiverUsername { get; set; }
        public string? ReceiverDisplayName { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }

    public class SendFriendRequestDto
    {
        [Required]
        public string? ReceiverId { get; set; }

        // Caregiver acting on behalf of recipient
        public string? ActingOnBehalfOf { get; set; }
    }
}
