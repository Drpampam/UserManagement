using UserManagement.Domain.Models;

namespace UserManagement.Core.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateToken(AppUser user);
        string GenerateRefreshToken();
    }
}
