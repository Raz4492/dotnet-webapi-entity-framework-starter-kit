using System.ComponentModel.DataAnnotations;

namespace SmartWebApi.Models.DTOs.Auth;

public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}