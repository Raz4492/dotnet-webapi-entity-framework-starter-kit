using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWebApi.Models.Common;
using SmartWebApi.Models.DTOs.Auth;
using SmartWebApi.Services.Interfaces;
using System.Security.Claims;

namespace SmartWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and return JWT tokens
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication response with access and refresh tokens</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Invalid request", 
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            ));
        }

        var result = await _authService.LoginAsync(request);
        if (result == null)
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid email or password"));
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Login successful"));
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Authentication response with access and refresh tokens</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Invalid request", 
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            ));
        }

        var result = await _authService.RegisterAsync(request);
        if (result == null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Registration failed. User may already exist."));
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Registration successful"));
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>New authentication response with updated tokens</returns>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Invalid request", 
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            ));
        }

        var result = await _authService.RefreshTokenAsync(request);
        if (result == null)
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid or expired refresh token"));
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Token refreshed successfully"));
    }

    /// <summary>
    /// Revoke a specific refresh token
    /// </summary>
    /// <param name="request">Refresh token to revoke</param>
    /// <returns>Success confirmation</returns>
    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Invalid request", 
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            ));
        }

        var result = await _authService.RevokeTokenAsync(request.RefreshToken);
        if (!result)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to revoke token"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, "Token revoked successfully"));
    }

    /// <summary>
    /// Revoke all refresh tokens for the current user
    /// </summary>
    /// <returns>Success confirmation</returns>
    [HttpPost("revoke-all")]
    [Authorize]
    public async Task<IActionResult> RevokeAllTokens()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user"));
        }

        var result = await _authService.RevokeAllTokensAsync(userId);
        if (!result)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to revoke all tokens"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, "All tokens revoked successfully"));
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    /// <returns>Current user details</returns>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var user = new
        {
            Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Email = User.FindFirst(ClaimTypes.Email)?.Value,
            FirstName = User.FindFirst(ClaimTypes.GivenName)?.Value,
            LastName = User.FindFirst(ClaimTypes.Surname)?.Value
        };

        return Ok(ApiResponse<object>.SuccessResponse(user, "User information retrieved successfully"));
    }
}