namespace Diversion.DTOs
{
    public class InterestDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
    }

    public class InterestWithSubInterestsDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public List<SubInterestDto> SubInterests { get; set; } = [];
    }
}
