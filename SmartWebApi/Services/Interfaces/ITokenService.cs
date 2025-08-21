using SmartWebApi.Models.Identity;
using System.Security.Claims;

namespace SmartWebApi.Services.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}