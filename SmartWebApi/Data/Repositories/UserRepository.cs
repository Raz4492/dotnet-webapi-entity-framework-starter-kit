using Microsoft.EntityFrameworkCore;
using SmartWebApi.Data.Repositories.Interfaces;
using SmartWebApi.Models.Identity;

namespace SmartWebApi.Data.Repositories;

public class UserRepository : Repository<ApplicationUser>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<ApplicationUser?> GetByIdWithRefreshTokensAsync(string id)
    {
        return await _dbSet
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<ApplicationUser>> GetActiveUsersAsync()
    {
        return await _dbSet
            .Where(u => u.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApplicationUser>> GetInactiveUsersAsync()
    {
        return await _dbSet
            .Where(u => !u.IsActive)
            .ToListAsync();
    }
}