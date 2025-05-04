using System.Security.Claims;
using UserService.Domain.Models;
using UserService.DTOs;
using UserService.Interfaces;

namespace UserService.Repositories
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IAccountRepository accountRepository,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AccountService> logger
            )
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Account Management
        public async Task<(bool Success, string Message, AppUser User)> RegisterUserAsync(RegisterUserDto model)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _accountRepository.GetByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return (false, "Email is already registered", null);
                }

                // Create new user
                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    CreatedTime = DateTime.UtcNow,
                    CreatedBy = GetCurrentUserId() ?? "SYSTEM",
                    IsActive = true
                };

                var result = await _accountRepository.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    return (false, $"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}", null);
                }

                // Log user creation event
                await _auditService.LogUserActivityAsync(
                    user.Id.ToString(),
                    "USER_REGISTERED",
                    new Dictionary<string, object>
                    {
                        ["Email"] = user.Email,
                        ["CreatedTime"] = user.CreatedTime
                    });

                return (true, "User registered successfully", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user with email {Email}", model.Email);
                return (false, "An error occurred during registration", null);
            }
        }

        #endregion
        public Task<(bool Success, string Message, AppUser User)> AuthenticateAsync(string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message)> DeactivateUserAsync(Guid userId, string reason)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message, string Token, string RefreshToken)> GenerateTokensAsync(AppUser user)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message, AppUser User)> GetUserByIdAsync(Guid userId, bool includeProfile = true)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message, IEnumerable<AppUser> Users)> GetUserListAsync(string searchTerm = null, bool? isActive = null, int page = 1, int pageSize = 10)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message)> ReactivateUserAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message, string Token, string RefreshToken)> RefreshTokenAsync(string expiredToken, string refreshToken)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message)> RequestPasswordResetAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message)> ResetPasswordAsync(string email, string token, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message)> RevokeTokenAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message)> UpdateUserProfileAsync(Guid userId, UserProfileDto model)
        {
            throw new NotImplementedException();
        }


        #region Helpers

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        #endregion
  
    }
}
