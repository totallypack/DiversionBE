using System.ComponentModel.DataAnnotations;

namespace Diversion.Models
{
    public class SubInterest
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public Guid InterestId { get; set; }
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public Interest Interest { get; set; }
        public ICollection<Event> Events { get; set; }
        public ICollection<UserInterest> UserInterests { get; set; }
    }
}
