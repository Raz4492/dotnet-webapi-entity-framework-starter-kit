using SmartWebApi.Models.DTOs;
using SmartWebApi.Models.Identity;

namespace SmartWebApi.Services.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<bool> UpdateLastLoginAsync(string userId);
    Task<bool> DeactivateUserAsync(string userId);
    Task<bool> ActivateUserAsync(string userId);
}