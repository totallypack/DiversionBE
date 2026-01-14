using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Diversion.Models
{
    public class UserProfile
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? ProfilePicUrl { get; set; }
        public DateTime? DOB {  get; set; }

        // User Type
        public UserType UserType { get; set; } = UserType.Regular;

        // Business-specific fields
        public string? BusinessName { get; set; }
        public string? BusinessWebsite { get; set; }
        public string? BusinessHours { get; set; }
        public string? BusinessCategory { get; set; }
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }

        // Caregiver-specific fields (only populated if UserType == Caregiver)
        [MaxLength(1000)]
        public string? CaregiverCredentials { get; set; }  // e.g., "CNA, Home Health Aide"

        public int? YearsOfExperience { get; set; }

        [MaxLength(500)]
        public string? Certifications { get; set; }  // Comma-separated: "CPR, First Aid, Dementia Care"

        [MaxLength(500)]
        public string? CareTypes { get; set; }  // Comma-separated: "Elderly, Disability, Post-Surgery"

        [MaxLength(500)]
        public string? Specializations { get; set; }  // "Alzheimer's, Mobility Assistance, Medication Management"

        public bool IsBackgroundChecked { get; set; } = false;

        [MaxLength(100)]
        public string? LicenseNumber { get; set; }

        public DateTime? LicenseExpiry { get; set; }

        [MaxLength(200)]
        public string? EmploymentStatus { get; set; }  // "Self-Employed", "Agency: [Name]", "Family Caregiver"

        public IdentityUser? User { get; set; }
        public ICollection<UserInterest> UserInterests { get; set; }
    }
}
