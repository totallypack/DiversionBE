using System.ComponentModel.DataAnnotations;

namespace Diversion.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(20, MinimumLength =3)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username must be alphanumeric")]
        public string? Username { get; set; }

        [Required]
        [StringLength (100, MinimumLength = 8)]
        public string? Password { get; set; }
    }
}