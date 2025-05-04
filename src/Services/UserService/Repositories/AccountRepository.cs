using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.Data;
using UserService.Domain.Models;
using UserService.Interfaces;

namespace UserService.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AccountRepository(
            AppDbContext context, 
            UserManager<AppUser> userManager
            )
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<AppUser> GetByIdAsync(Guid id, bool includeProfile = false)
        {
            var query = _userManager.Users;

            if (includeProfile) 
                query = query.Include(Q => Q.Profile);

            return await query.FirstOrDefaultAsync(Q => Q.Id == id);
        }


        public async Task<AppUser> GetByEmailAsync(string email, bool includeProfile = false)
        {
            var query = _userManager.Users;

            if (!includeProfile)
                query = query.Include(Q => Q.Profile);

            return await query.FirstOrDefaultAsync(Q => Q.Email == email);
        }

        public async Task<bool> CheckPasswordAsync(AppUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password); ;
        }

        public async Task<IdentityResult> CreateAsync(AppUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<IdentityResult> UpdateAsync(AppUser user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(AppUser user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(AppUser user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(AppUser user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordAsync(AppUser user, string token, string newPassword)
        {
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task UpdateRefreshToken(AppUser user, string refreshToken, DateTime expiry)
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = expiry;
            await _userManager.UpdateAsync(user);
        }
    }
}
