using Microsoft.AspNetCore.Identity;
using UserService.Domain.Models;

namespace UserService.Interfaces
{
    public interface IAccountRepository
    {
        Task<AppUser> GetByIdAsync(Guid id);
        Task<AppUser> GetByEmailAsync(string email);
        Task<bool> CheckPasswordAsync(AppUser user, string password);
        Task<IdentityResult> CreateAsync(AppUser user, string password);
        Task<IdentityResult> UpdateAsync(AppUser user);
        Task<string> GenerateEmailConfirmationTokenAsync(AppUser user);
        Task<IdentityResult> ConfirmEmailAsync(AppUser user, string token);
        Task<string> GeneratePasswordResetTokenAsync(AppUser user);
        Task<IdentityResult> ResetPasswordAsync(AppUser user, string token, string newPassword);
        Task UpdateRefreshToken(AppUser user, string refreshToken, DateTime expiry);

    }
}
