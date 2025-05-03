using Common.Entities;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Domain.Models
{
    public class UserProfile : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [PersonalData]
        [StringLength(100)]
        public string FullName { get; set; }

        [PersonalData]
        [StringLength(16)]
        public string IdentityNumber { get; set; }

        [PersonalData]
        [StringLength(16)]
        public string TaxNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }

        [PersonalData]
        [StringLength(10)]
        public string Gender { get; set; }

        [PersonalData]
        [StringLength(200)]
        public string Address { get; set; }

        [PersonalData]
        [StringLength(50)]
        public string City { get; set; }

        [PersonalData]
        [StringLength(50)]
        public string State { get; set; }

        [PersonalData]
        [StringLength(50)]
        public string Country { get; set; }

        [PersonalData]
        [StringLength(20)]
        public string ZipCode { get; set; }

        [PersonalData]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        // Profile Settings
        [StringLength(255)]
        public string ProfilePictureUrl { get; set; }

        public bool EmailNotifications { get; set; } = true;

        public bool PushNotifications { get; set; } = true;

        public bool TwoFactorEnabled { get; set; } = false;

        // Social Media Links
        [StringLength(255)]
        [Url]
        public string? LinkedInUrl { get; set; }

        [StringLength(255)]
        [Url]
        public string? TwitterUrl { get; set; }

        [StringLength(255)]
        [Url]
        public string? FacebookUrl { get; set; }

        [StringLength(255)]
        [Url]
        public string? InstagramUrl { get; set; }

        // Privacy Settings
        public bool ProfileVisibility { get; set; } = true;

        public bool ShowEmail { get; set; } = false;

        public bool ShowBirthDate { get; set; } = false;

        // Status information
        public bool IsProfileComplete { get; set; } = false;

        // Navigation property back to AppUser
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }

        // Calculated properties
        [NotMapped]
        public int Age => DateOfBirth.HasValue ?
            (DateTime.Today.Year - DateOfBirth.Value.Year -
            (DateOfBirth.Value.Date > DateTime.Today.AddYears(-DateTime.Today.Year + DateOfBirth.Value.Year) ? 1 : 0)) : 0;

    }
}
