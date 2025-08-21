using FluentValidation;
using SmartWebApi.Models.DTOs.Auth;
using SmartWebApi.Validators;

namespace SmartWebApi.Extensions;

public static class FluentValidationExtensions
{
    public static IServiceCollection AddFluentValidationServices(this IServiceCollection services)
    {
        services.AddScoped<IValidator<LoginRequestDto>, LoginRequestValidator>();
        services.AddScoped<IValidator<RegisterRequestDto>, RegisterRequestValidator>();
        services.AddScoped<IValidator<RefreshTokenRequestDto>, RefreshTokenRequestValidator>();

        return services;
    }
}