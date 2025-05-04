using UserService.Domain.Models;
using UserService.DTOs;

namespace UserService.Interfaces
{
    public interface IAccountService
    {
        // Account Management
        Task<(bool Success, string Message, AppUser User)> RegisterUserAsync(RegisterUserDto model);
        Task<(bool Success, string Message, AppUser User)> AuthenticateAsync(string email, string password);
        Task<(bool Success, string Message)> UpdateUserProfileAsync(Guid userId, UserProfileDto model);
        Task<(bool Success, string Message)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
        Task<(bool Success, string Message)> RequestPasswordResetAsync(string email);
        Task<(bool Success, string Message)> ResetPasswordAsync(string email, string token, string newPassword);

        // User Management
        Task<(bool Success, string Message, AppUser User)> GetUserByIdAsync(Guid userId, bool includeProfile = true);
        Task<(bool Success, string Message)> DeactivateUserAsync(Guid userId, string reason);
        Task<(bool Success, string Message)> ReactivateUserAsync(Guid userId);
        Task<(bool Success, string Message, IEnumerable<AppUser> Users)> GetUserListAsync(
            string searchTerm = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 10);

        // Token Management
        Task<(bool Success, string Message, string Token, string RefreshToken)> GenerateTokensAsync(AppUser user);
        Task<(bool Success, string Message, string Token, string RefreshToken)> RefreshTokenAsync(string expiredToken, string refreshToken);
        Task<(bool Success, string Message)> RevokeTokenAsync(Guid userId);
    }
}
