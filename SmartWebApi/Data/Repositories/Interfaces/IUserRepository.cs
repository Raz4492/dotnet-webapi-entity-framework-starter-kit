using SmartWebApi.Models.Identity;

namespace SmartWebApi.Data.Repositories.Interfaces;

public interface IUserRepository : IRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<ApplicationUser?> GetByIdWithRefreshTokensAsync(string id);
    Task<bool> EmailExistsAsync(string email);
    Task<IEnumerable<ApplicationUser>> GetActiveUsersAsync();
    Task<IEnumerable<ApplicationUser>> GetInactiveUsersAsync();
}