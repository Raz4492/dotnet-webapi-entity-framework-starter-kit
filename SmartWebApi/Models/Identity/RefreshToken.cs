using System.ComponentModel.DataAnnotations;

namespace SmartWebApi.Models.Identity;

public class RefreshToken
{
    public int Id { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiryDate { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public virtual ApplicationUser User { get; set; } = null!;

    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RevokedBy { get; set; }
    public DateTime? RevokedAt { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsActive => !IsRevoked && !IsExpired;
}