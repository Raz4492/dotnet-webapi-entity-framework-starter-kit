using FluentValidation;
using SmartWebApi.Models.DTOs.Auth;

namespace SmartWebApi.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}