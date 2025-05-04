using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class UserProfileDto
    {
        public string FullName { get; set; }

        public string IdentityNumber { get; set; }

        public string TaxNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string ZipCode { get; set; }

        public string PhoneNumber { get; set; }

        public string ProfilePictureUrl { get; set; }

        public bool EmailNotifications { get; set; } = true;

        public bool PushNotifications { get; set; } = true;

        public bool TwoFactorEnabled { get; set; } = false;

        public string? LinkedInUrl { get; set; }

        public string? TwitterUrl { get; set; }

        public string? FacebookUrl { get; set; }

        public string? InstagramUrl { get; set; }
    }
}
