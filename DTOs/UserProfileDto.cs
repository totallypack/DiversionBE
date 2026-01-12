namespace Diversion.DTOs
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public DateTime? DOB { get; set; }
        public string? ProfilePicUrl { get; set; }
        public string UserType { get; set; } = "Regular";
        public string? BusinessName { get; set; }
        public string? BusinessWebsite { get; set; }
        public string? BusinessHours { get; set; }
        public string? BusinessCategory { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }

    public class CreateUserProfileDto
    {
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public DateTime? DOB { get; set; }
        public string? ProfilePicUrl { get; set; }
        public string? UserType { get; set; }
        public string? BusinessName { get; set; }
        public string? BusinessWebsite { get; set; }
        public string? BusinessHours { get; set; }
        public string? BusinessCategory { get; set; }
    }

    public class UpdateUserProfileDto
    {
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public DateTime? DOB { get; set; }
        public string? ProfilePicUrl { get; set; }
        public string? UserType { get; set; }
        public string? BusinessName { get; set; }
        public string? BusinessWebsite { get; set; }
        public string? BusinessHours { get; set; }
        public string? BusinessCategory { get; set; }
    }

    public class UserProfileWithInterestsDto
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public DateTime? DOB { get; set; }
        public string? ProfilePicUrl { get; set; }
        public string UserType { get; set; } = "Regular";
        public string? BusinessName { get; set; }
        public string? BusinessWebsite { get; set; }
        public string? BusinessHours { get; set; }
        public string? BusinessCategory { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public List<SubInterestWithInterestDto> Interests { get; set; } = [];
    }
}
