namespace Diversion.DTOs
{
    public class SubInterestDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid InterestId { get; set; }
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
    }

    public class SubInterestWithInterestDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public InterestDto Interest { get; set; }
    }
}
