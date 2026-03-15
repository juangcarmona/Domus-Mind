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
using DomusMind.Application.Features.Responsibilities.AssignPrimaryOwner;
using DomusMind.Application.Features.Responsibilities.AssignSecondaryOwner;
using DomusMind.Application.Features.Responsibilities.CreateResponsibilityDomain;
using DomusMind.Application.Features.Responsibilities.TransferResponsibility;
using DomusMind.Application.Features.Calendar.ScheduleEvent;
using DomusMind.Application.Features.Calendar.RescheduleEvent;
using DomusMind.Application.Features.Calendar.CancelEvent;
using DomusMind.Application.Features.Calendar.AddEventParticipant;
using DomusMind.Application.Features.Calendar.RemoveEventParticipant;
using DomusMind.Application.Features.Calendar.AddReminder;
using DomusMind.Application.Features.Calendar.RemoveReminder;
using DomusMind.Application.Features.Calendar.GetFamilyTimeline;
using DomusMind.Contracts.Auth;
using DomusMind.Contracts.Calendar;
using DomusMind.Contracts.Family;
using DomusMind.Contracts.Responsibilities;
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

        // Responsibilities slices
        services.AddScoped<ICommandHandler<CreateResponsibilityDomainCommand, CreateResponsibilityDomainResponse>, CreateResponsibilityDomainCommandHandler>();
        services.AddScoped<ICommandHandler<AssignPrimaryOwnerCommand, AssignPrimaryOwnerResponse>, AssignPrimaryOwnerCommandHandler>();
        services.AddScoped<ICommandHandler<AssignSecondaryOwnerCommand, AssignSecondaryOwnerResponse>, AssignSecondaryOwnerCommandHandler>();
        services.AddScoped<ICommandHandler<TransferResponsibilityCommand, TransferResponsibilityResponse>, TransferResponsibilityCommandHandler>();

        // Calendar slices
        services.AddScoped<ICommandHandler<ScheduleEventCommand, ScheduleEventResponse>, ScheduleEventCommandHandler>();
        services.AddScoped<ICommandHandler<RescheduleEventCommand, RescheduleEventResponse>, RescheduleEventCommandHandler>();
        services.AddScoped<ICommandHandler<CancelEventCommand, CancelEventResponse>, CancelEventCommandHandler>();
        services.AddScoped<ICommandHandler<AddEventParticipantCommand, AddEventParticipantResponse>, AddEventParticipantCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveEventParticipantCommand, RemoveEventParticipantResponse>, RemoveEventParticipantCommandHandler>();
        services.AddScoped<ICommandHandler<AddReminderCommand, AddReminderResponse>, AddReminderCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveReminderCommand, RemoveReminderResponse>, RemoveReminderCommandHandler>();
        services.AddScoped<IQueryHandler<GetFamilyTimelineQuery, FamilyTimelineResponse>, GetFamilyTimelineQueryHandler>();

        return services;
    }
}
