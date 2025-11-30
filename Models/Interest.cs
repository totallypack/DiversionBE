using System.ComponentModel.DataAnnotations;

namespace Diversion.Models
{
    public class Interest
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        public string? IconUrl { get; set; }

        // Navigation properties
        public ICollection<SubInterest> SubInterests { get; set; }
    }
}
