using System.Text;
using DomusMind.Application.Abstractions.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DomusMind.Infrastructure.Auth;

public static class AuthInfrastructureExtensions
{
    public static IServiceCollection AddDomusMindAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.SigningKey),
                "JWT signing key is required. Set Jwt:SigningKey in configuration.")
            .Validate(
                o => string.IsNullOrWhiteSpace(o.SigningKey) || o.SigningKey.Length >= 32,
                "JWT signing key must be at least 32 characters.")
            .ValidateOnStart();

        services
            .AddOptions<BootstrapAdminOptions>()
            .Bind(configuration.GetSection(BootstrapAdminOptions.SectionName))
            .Validate(
                o => !o.Enabled || !string.IsNullOrWhiteSpace(o.Email),
                "BootstrapAdmin:Email is required when BootstrapAdmin:Enabled is true.")
            .Validate(
                o => !o.Enabled || !string.IsNullOrWhiteSpace(o.Password),
                "BootstrapAdmin:Password is required when BootstrapAdmin:Enabled is true.")
            .ValidateOnStart();

        var jwtSection = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "Jwt configuration section is missing. Ensure Jwt:SigningKey is set.");

        if (string.IsNullOrWhiteSpace(jwtSection.SigningKey))
            throw new InvalidOperationException(
                "JWT signing key is missing. Set Jwt:SigningKey in configuration.");

        if (jwtSection.SigningKey.Length < 32)
            throw new InvalidOperationException(
                "JWT signing key is too weak. It must be at least 32 characters.");

        var keyBytes = Encoding.UTF8.GetBytes(jwtSection.SigningKey);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSection.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSection.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ClockSkew = TimeSpan.Zero,
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserAccessor>();
        services.AddScoped<IAccessTokenGenerator, AccessTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IRefreshTokenService, RefreshTokenStore>();
        services.AddScoped<IAuthUserRepository, AuthUserRepository>();
        services.AddScoped<IFamilyAuthorizationService, FamilyAuthorizationService>();
        services.AddScoped<IFamilyAccessGranter, FamilyAccessGranter>();

        return services;
    }

    public static IServiceCollection AddDomusMindAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        return services;
    }
}
