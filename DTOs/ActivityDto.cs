namespace Diversion.DTOs
{
    public class ActivityDto
    {
        public string? ActivityType { get; set; }
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? DisplayName { get; set; }
        public DateTime Timestamp { get; set; }

        public Guid? EventId { get; set; }
        public string? EventTitle { get; set; }
        public string? EventType { get; set; }
        public string? RsvpStatus { get; set; }

        public Guid? SubInterestId { get; set; }
        public string? SubInterestName { get; set; }
        public string? InterestName { get; set; }
    }
}
