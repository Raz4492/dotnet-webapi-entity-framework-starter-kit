using SmartWebApi.Models.Identity;

namespace SmartWebApi.Data.Repositories.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);
    Task<IEnumerable<RefreshToken>> GetExpiredTokensAsync();
    Task RevokeTokenAsync(string token);
    Task RevokeAllUserTokensAsync(string userId);
    Task CleanupExpiredTokensAsync();
}