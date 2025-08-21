using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWebApi.Models.Common;
using SmartWebApi.Models.DTOs;
using SmartWebApi.Services.Interfaces;
using System.Security.Claims;

namespace SmartWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User information</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId != id)
        {
            return Forbid();
        }

        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User retrieved successfully"));
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    /// <returns>Current user information</returns>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user"));
        }

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Profile retrieved successfully"));
    }

    /// <summary>
    /// Deactivate current user account
    /// </summary>
    /// <returns>Success confirmation</returns>
    [HttpPost("deactivate")]
    public async Task<IActionResult> DeactivateAccount()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user"));
        }

        var result = await _userService.DeactivateUserAsync(userId);
        if (!result)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to deactivate account"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, "Account deactivated successfully"));
    }

    /// <summary>
    /// Reactivate user account (admin only)
    /// </summary>
    /// <param name="id">User ID to reactivate</param>
    /// <returns>Success confirmation</returns>
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateAccount(string id)
    {
        // Note: In a real application, you'd check for admin role here
        var result = await _userService.ActivateUserAsync(id);
        if (!result)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to activate account"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, "Account activated successfully"));
    }
}