using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SmartWebApi.Data.UnitOfWork;
using SmartWebApi.Models.DTOs;
using SmartWebApi.Models.Identity;
using SmartWebApi.Services.Interfaces;

namespace SmartWebApi.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UserService> _logger;
    private readonly IConfiguration _configuration;

    public UserService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cacheService,
        ILogger<UserService> logger,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        try
        {
            var cacheKey = $"user:{userId}";
            var cachedUser = await _cacheService.GetAsync<UserDto>(cacheKey);
            
            if (cachedUser != null)
            {
                return cachedUser;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var userDto = _mapper.Map<UserDto>(user);
            
            var cacheExpiry = TimeSpan.FromMinutes(_configuration.GetValue<int>("CacheSettings:UserCacheExpirationMinutes"));
            await _cacheService.SetAsync(cacheKey, userDto, cacheExpiry);

            return userDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            return null;
        }
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        try
        {
            var cacheKey = $"user:email:{email}";
            var cachedUser = await _cacheService.GetAsync<UserDto>(cacheKey);
            
            if (cachedUser != null)
            {
                return cachedUser;
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return null;
            }

            var userDto = _mapper.Map<UserDto>(user);
            
            var cacheExpiry = TimeSpan.FromMinutes(_configuration.GetValue<int>("CacheSettings:UserCacheExpirationMinutes"));
            await _cacheService.SetAsync(cacheKey, userDto, cacheExpiry);

            return userDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            return null;
        }
    }

    public async Task<bool> UpdateLastLoginAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.LastLoginAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Clear cache
                await _cacheService.RemoveAsync($"user:{userId}");
                await _cacheService.RemoveAsync($"user:email:{user.Email}");
            }

            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> DeactivateUserAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Clear cache and revoke all tokens
                await _cacheService.RemoveAsync($"user:{userId}");
                await _cacheService.RemoveAsync($"user:email:{user.Email}");
                
                await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId);
                await _unitOfWork.SaveChangesAsync();
            }

            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ActivateUserAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Clear cache
                await _cacheService.RemoveAsync($"user:{userId}");
                await _cacheService.RemoveAsync($"user:email:{user.Email}");
            }

            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user: {UserId}", userId);
            return false;
        }
    }
}