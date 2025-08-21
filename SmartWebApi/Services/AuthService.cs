using Microsoft.AspNetCore.Identity;
using SmartWebApi.Data.UnitOfWork;
using SmartWebApi.Models.DTOs;
using SmartWebApi.Models.DTOs.Auth;
using SmartWebApi.Models.Identity;
using SmartWebApi.Services.Interfaces;
using AutoMapper;

namespace SmartWebApi.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IUserService userService,
        IMapper mapper,
        ILogger<AuthService> logger,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _userService = userService;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Login attempt failed for email: {Email}", request.Email);
                return null;
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Invalid password for user: {Email}", request.Email);
                return null;
            }

            await _userService.UpdateLastLoginAsync(user.Id);

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays"))
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {Email} logged in successfully", request.Email);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:AccessTokenExpirationMinutes")),
                RefreshTokenExpiry = refreshTokenEntity.ExpiryDate,
                User = _mapper.Map<UserDto>(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return null;
        }
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                return null;
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                _logger.LogWarning("User registration failed for email: {Email}. Errors: {Errors}", 
                    request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return null;
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays"))
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {Email} registered successfully", request.Email);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:AccessTokenExpirationMinutes")),
                RefreshTokenExpiry = refreshTokenEntity.ExpiryDate,
                User = _mapper.Map<UserDto>(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
            return null;
        }
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        try
        {
            var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);
            if (refreshToken == null || !refreshToken.IsActive)
            {
                _logger.LogWarning("Invalid or expired refresh token used");
                return null;
            }

            var user = await _userManager.FindByIdAsync(refreshToken.UserId);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("User not found or inactive for refresh token");
                return null;
            }

            await _unitOfWork.RefreshTokens.RevokeTokenAsync(request.RefreshToken);

            var accessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays"))
            };

            await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Token refreshed for user: {UserId}", user.Id);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:AccessTokenExpirationMinutes")),
                RefreshTokenExpiry = newRefreshTokenEntity.ExpiryDate,
                User = _mapper.Map<UserDto>(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return null;
        }
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        try
        {
            await _unitOfWork.RefreshTokens.RevokeTokenAsync(refreshToken);
            var result = await _unitOfWork.SaveChangesAsync() > 0;
            if (result)
            {
                _logger.LogInformation("Refresh token revoked");
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token");
            return false;
        }
    }

    public async Task<bool> RevokeAllTokensAsync(string userId)
    {
        try
        {
            await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId);
            var result = await _unitOfWork.SaveChangesAsync() > 0;
            if (result)
            {
                _logger.LogInformation("All refresh tokens revoked for user: {UserId}", userId);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all tokens for user: {UserId}", userId);
            return false;
        }
    }
}