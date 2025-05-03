using System.Security.Claims;
using UserService.Domain.Models;

namespace UserService.Interfaces
{
    public interface IJwtService
    {
        string GenerateJwtToken(AppUser user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
