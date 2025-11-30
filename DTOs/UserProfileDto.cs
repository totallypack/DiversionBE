namespace Diversion.DTOs
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public DateTime? DOB { get; set; }
        public string? ProfilePicUrl { get; set; }
    }

    public class CreateUserProfileDto
    {
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public DateTime? DOB { get; set; }
        public string? ProfilePicUrl { get; set; }
    }

    public class UpdateUserProfileDto
    {
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public DateTime? DOB { get; set; }
        public string? ProfilePicUrl { get; set; }
    }

    public class UserProfileWithInterestsDto
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public DateTime? DOB { get; set; }
        public string? ProfilePicUrl { get; set; }
        public List<SubInterestDto> Interests { get; set; } = [];
    }
}
