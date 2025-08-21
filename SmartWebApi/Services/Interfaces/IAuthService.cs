using SmartWebApi.Models.DTOs;
using SmartWebApi.Models.DTOs.Auth;

namespace SmartWebApi.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<bool> RevokeAllTokensAsync(string userId);
}