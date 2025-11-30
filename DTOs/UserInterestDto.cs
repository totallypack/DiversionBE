namespace Diversion.DTOs
{
    public class UserInterestDto
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public Guid SubInterestId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AddUserInterestDto
    {
        public Guid SubInterestId { get; set; }
    }

    public class UserInterestWithDetailsDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public SubInterestWithInterestDto SubInterest { get; set; }
    }
}
