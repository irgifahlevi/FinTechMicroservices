using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace UserService.Domain.Models
{
    public class AppUser : IdentityUser<Guid>
    {
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedTime { get; set; }
        public bool IsActive { get; set; } = true;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public virtual UserProfile Profile { get; set; }
    }
}
