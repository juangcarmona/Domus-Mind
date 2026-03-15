using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Features.Auth.ChangePassword;
using DomusMind.Application.Features.Auth.GetCurrentUser;
using DomusMind.Application.Features.Auth.Login;
using DomusMind.Application.Features.Auth.Logout;
using DomusMind.Application.Features.Auth.RefreshToken;
using DomusMind.Application.Features.Auth.RegisterUser;
using DomusMind.Application.Features.Family.AddMember;
using DomusMind.Application.Features.Family.CreateFamily;
using DomusMind.Application.Features.Family.GetFamily;
using DomusMind.Application.Features.Family.GetFamilyMembers;
using DomusMind.Contracts.Auth;
using DomusMind.Contracts.Family;
using Microsoft.Extensions.DependencyInjection;

namespace DomusMind.Application.DependencyInjection;

public static class ApplicationServices
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // Auth slices
        services.AddScoped<ICommandHandler<RegisterUserCommand, RegisterUserResponse>, RegisterUserCommandHandler>();
        services.AddScoped<ICommandHandler<LoginCommand, LoginResponse>, LoginCommandHandler>();
        services.AddScoped<ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>, RefreshTokenCommandHandler>();
        services.AddScoped<ICommandHandler<ChangePasswordCommand, ChangePasswordResponse>, ChangePasswordCommandHandler>();
        services.AddScoped<ICommandHandler<LogoutCommand, LogoutResponse>, LogoutCommandHandler>();
        services.AddScoped<IQueryHandler<GetCurrentUserQuery, MeResponse>, GetCurrentUserQueryHandler>();

        // Family slices
        services.AddScoped<ICommandHandler<CreateFamilyCommand, CreateFamilyResponse>, CreateFamilyCommandHandler>();
        services.AddScoped<ICommandHandler<AddMemberCommand, AddMemberResponse>, AddMemberCommandHandler>();
        services.AddScoped<IQueryHandler<GetFamilyQuery, FamilyResponse>, GetFamilyQueryHandler>();
        services.AddScoped<IQueryHandler<GetFamilyMembersQuery, IReadOnlyCollection<FamilyMemberResponse>>, GetFamilyMembersQueryHandler>();

        return services;
    }
}
