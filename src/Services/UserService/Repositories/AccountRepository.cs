using Microsoft.AspNetCore.Identity;
using UserService.Data;
using UserService.Domain.Models;
using UserService.Interfaces;

namespace UserService.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AccountRepository(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Task<bool> CheckPasswordAsync(AppUser user, string password)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> ConfirmEmailAsync(AppUser user, string token)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> CreateAsync(AppUser user, string password)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateEmailConfirmationTokenAsync(AppUser user)
        {
            throw new NotImplementedException();
        }

        public Task<string> GeneratePasswordResetTokenAsync(AppUser user)
        {
            throw new NotImplementedException();
        }

        public Task<AppUser> GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<AppUser> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> ResetPasswordAsync(AppUser user, string token, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(AppUser user)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRefreshToken(AppUser user, string refreshToken, DateTime expiry)
        {
            throw new NotImplementedException();
        }
    }
}
