using Microsoft.EntityFrameworkCore;
using SmartWebApi.Data.Repositories.Interfaces;
using SmartWebApi.Models.Identity;

namespace SmartWebApi.Data.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _dbSet
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<IEnumerable<RefreshToken>> GetExpiredTokensAsync()
    {
        return await _dbSet
            .Where(rt => rt.ExpiryDate <= DateTime.UtcNow && !rt.IsRevoked)
            .ToListAsync();
    }

    public async Task RevokeTokenAsync(string token)
    {
        var refreshToken = await _dbSet.FirstOrDefaultAsync(rt => rt.Token == token);
        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
        }
    }

    public async Task RevokeAllUserTokensAsync(string userId)
    {
        var userTokens = await _dbSet
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in userTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }
    }

    public async Task CleanupExpiredTokensAsync()
    {
        var expiredTokens = await GetExpiredTokensAsync();
        RemoveRange(expiredTokens);
    }
}