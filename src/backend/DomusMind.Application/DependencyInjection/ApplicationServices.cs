using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Features.Auth.ChangePassword;
using DomusMind.Application.Features.Auth.GetCurrentUser;
using DomusMind.Application.Features.Auth.Login;
using DomusMind.Application.Features.Auth.Logout;
using DomusMind.Application.Features.Auth.RefreshToken;
using DomusMind.Application.Features.Auth.RegisterUser;
using DomusMind.Application.Features.Calendar.AddEventParticipant;
using DomusMind.Application.Features.Calendar.AddReminder;
using DomusMind.Application.Features.Calendar.CancelEvent;
using DomusMind.Application.Features.Calendar.DetectCalendarConflicts;
using DomusMind.Application.Features.Calendar.GetFamilyPlans;
using DomusMind.Application.Features.Calendar.GetFamilyTimeline;
using DomusMind.Application.Features.Calendar.ProposeAlternativeTimes;
using DomusMind.Application.Features.Calendar.RemoveEventParticipant;
using DomusMind.Application.Features.Calendar.RemoveReminder;
using DomusMind.Application.Features.Calendar.RescheduleEvent;
using DomusMind.Application.Features.Calendar.ScheduleEvent;
using DomusMind.Application.Features.Calendar.SuggestEventParticipants;
using DomusMind.Application.Features.Family.AddMember;
using DomusMind.Application.Features.Family.CompleteOnboarding;
using DomusMind.Application.Features.Family.CreateFamily;
using DomusMind.Application.Features.Family.UpdateFamilySettings;
using DomusMind.Application.Features.Family.GetEnrichedTimeline;
using DomusMind.Application.Features.Family.GetFamily;
using DomusMind.Application.Features.Family.GetWeeklyGrid;
using DomusMind.Application.Features.Family.GetFamilyMembers;
using DomusMind.Application.Features.Family.GetHouseholdTimeline;
using DomusMind.Application.Features.Family.GetMemberActivity;
using DomusMind.Application.Features.Family.GetMemberDetails;
using DomusMind.Application.Features.Family.GetMyFamily;
using DomusMind.Application.Features.Family.InviteMember;
using DomusMind.Application.Features.Family.LinkMemberAccount;
using DomusMind.Application.Features.Family.UpdateMember;
using DomusMind.Application.Features.Family.UpdateMemberProfile;
using DomusMind.Application.Features.Family.DisableMemberAccess;
using DomusMind.Application.Features.Family.EnableMemberAccess;
using DomusMind.Application.Features.Family.ProvisionMemberAccess;
using DomusMind.Application.Features.Family.RegenerateTemporaryPassword;
using DomusMind.Application.Features.Languages.GetSupportedLanguages;
using DomusMind.Application.Features.Setup.GetSetupStatus;
using DomusMind.Application.Features.Setup.InitializeSystem;
using DomusMind.Application.Features.Responsibilities.AssignPrimaryOwner;
using DomusMind.Application.Features.Responsibilities.AssignSecondaryOwner;
using DomusMind.Application.Features.Responsibilities.CreateResponsibilityDomain;
using DomusMind.Application.Features.Responsibilities.DetectResponsibilityOverload;
using DomusMind.Application.Features.Responsibilities.GetHouseholdAreas;
using DomusMind.Application.Features.Responsibilities.GetResponsibilityBalance;
using DomusMind.Application.Features.Responsibilities.GetResponsibilityVisibility;
using DomusMind.Application.Features.Responsibilities.RemoveSecondaryOwner;
using DomusMind.Application.Features.Responsibilities.RenameResponsibilityDomain;
using DomusMind.Application.Features.Responsibilities.SuggestResponsibilityOwner;
using DomusMind.Application.Features.Responsibilities.TransferResponsibility;
using DomusMind.Application.Features.Responsibilities.UpdateResponsibilityDomainColor;
using DomusMind.Application.Features.Tasks.AssignTask;
using DomusMind.Application.Features.Tasks.CancelTask;
using DomusMind.Application.Features.Tasks.CompleteTask;
using DomusMind.Application.Features.Tasks.CreateRoutine;
using DomusMind.Application.Features.Tasks.CreateTask;
using DomusMind.Application.Features.Tasks.GetRoutinesByFamily;
using DomusMind.Application.Features.Tasks.PauseRoutine;
using DomusMind.Application.Features.Tasks.ReassignTask;
using DomusMind.Application.Features.Tasks.RescheduleTask;
using DomusMind.Application.Features.Tasks.ResumeRoutine;
using DomusMind.Application.Features.Tasks.UpdateRoutine;
using DomusMind.Application.Features.SharedLists.CreateSharedList;
using DomusMind.Application.Features.SharedLists.AddItemToSharedList;
using DomusMind.Application.Features.SharedLists.ToggleSharedListItem;
using DomusMind.Application.Features.SharedLists.GetFamilySharedLists;
using DomusMind.Application.Features.SharedLists.GetSharedListDetail;
using DomusMind.Application.Features.SharedLists.UpdateSharedListItem;
using DomusMind.Application.Features.SharedLists.RemoveSharedListItem;
using DomusMind.Application.Features.SharedLists.LinkSharedList;
using DomusMind.Application.Features.SharedLists.UnlinkSharedList;
using DomusMind.Application.Features.SharedLists.CreateLinkedSharedListForEvent;
using DomusMind.Application.Features.SharedLists.GetSharedListByLinkedEntity;
using DomusMind.Application.Features.SharedLists.RenameSharedList;
using DomusMind.Application.Features.SharedLists.DeleteSharedList;
using DomusMind.Application.Features.SharedLists.ReorderSharedListItems;
using DomusMind.Contracts.Auth;
using DomusMind.Contracts.Calendar;
using DomusMind.Contracts.Family;
using DomusMind.Contracts.Languages;
using DomusMind.Contracts.Responsibilities;
using DomusMind.Contracts.Setup;
using DomusMind.Contracts.Tasks;
using DomusMind.Contracts.SharedLists;
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
        services.AddScoped<ICommandHandler<CompleteOnboardingCommand, CompleteOnboardingResponse>, CompleteOnboardingCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateFamilySettingsCommand, UpdateFamilySettingsResponse>, UpdateFamilySettingsCommandHandler>();
        services.AddScoped<ICommandHandler<InviteMemberCommand, InviteMemberResponse>, InviteMemberCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateMemberCommand, UpdateMemberResponse>, UpdateMemberCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateMemberProfileCommand, UpdateMemberProfileResponse>, UpdateMemberProfileCommandHandler>();
        services.AddScoped<ICommandHandler<LinkMemberAccountCommand, LinkMemberAccountResponse>, LinkMemberAccountCommandHandler>();
        services.AddScoped<ICommandHandler<ProvisionMemberAccessCommand, ProvisionMemberAccessResponse>, ProvisionMemberAccessCommandHandler>();
        services.AddScoped<ICommandHandler<RegenerateTemporaryPasswordCommand, RegenerateTemporaryPasswordResponse>, RegenerateTemporaryPasswordCommandHandler>();
        services.AddScoped<ICommandHandler<DisableMemberAccessCommand, DisableMemberAccessResponse>, DisableMemberAccessCommandHandler>();
        services.AddScoped<ICommandHandler<EnableMemberAccessCommand, EnableMemberAccessResponse>, EnableMemberAccessCommandHandler>();
        services.AddScoped<IQueryHandler<GetFamilyQuery, FamilyResponse>, GetFamilyQueryHandler>();
        services.AddScoped<IQueryHandler<GetMyFamilyQuery, FamilyResponse>, GetMyFamilyQueryHandler>();
        services.AddScoped<IQueryHandler<GetFamilyMembersQuery, IReadOnlyCollection<MemberDirectoryItemResponse>>, GetFamilyMembersQueryHandler>();
        services.AddScoped<IQueryHandler<GetMemberDetailsQuery, MemberDetailResponse>, GetMemberDetailsQueryHandler>();
        services.AddScoped<IQueryHandler<GetHouseholdTimelineQuery, HouseholdTimelineResponse>, GetHouseholdTimelineQueryHandler>();

        // Responsibilities slices
        services.AddScoped<ICommandHandler<CreateResponsibilityDomainCommand, CreateResponsibilityDomainResponse>, CreateResponsibilityDomainCommandHandler>();
        services.AddScoped<ICommandHandler<AssignPrimaryOwnerCommand, AssignPrimaryOwnerResponse>, AssignPrimaryOwnerCommandHandler>();
        services.AddScoped<ICommandHandler<AssignSecondaryOwnerCommand, AssignSecondaryOwnerResponse>, AssignSecondaryOwnerCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveSecondaryOwnerCommand, RemoveSecondaryOwnerResponse>, RemoveSecondaryOwnerCommandHandler>();
        services.AddScoped<ICommandHandler<TransferResponsibilityCommand, TransferResponsibilityResponse>, TransferResponsibilityCommandHandler>();
        services.AddScoped<ICommandHandler<RenameResponsibilityDomainCommand, RenameResponsibilityDomainResponse>, RenameResponsibilityDomainCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateResponsibilityDomainColorCommand, UpdateResponsibilityDomainColorResponse>, UpdateResponsibilityDomainColorCommandHandler>();

        // Calendar slices
        services.AddScoped<ICommandHandler<ScheduleEventCommand, ScheduleEventResponse>, ScheduleEventCommandHandler>();
        services.AddScoped<ICommandHandler<RescheduleEventCommand, RescheduleEventResponse>, RescheduleEventCommandHandler>();
        services.AddScoped<ICommandHandler<CancelEventCommand, CancelEventResponse>, CancelEventCommandHandler>();
        services.AddScoped<ICommandHandler<AddEventParticipantCommand, AddEventParticipantResponse>, AddEventParticipantCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveEventParticipantCommand, RemoveEventParticipantResponse>, RemoveEventParticipantCommandHandler>();
        services.AddScoped<ICommandHandler<AddReminderCommand, AddReminderResponse>, AddReminderCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveReminderCommand, RemoveReminderResponse>, RemoveReminderCommandHandler>();
        services.AddScoped<IQueryHandler<GetFamilyTimelineQuery, FamilyTimelineResponse>, GetFamilyTimelineQueryHandler>();

        // Tasks slices
        services.AddScoped<ICommandHandler<CreateTaskCommand, CreateTaskResponse>, CreateTaskCommandHandler>();
        services.AddScoped<ICommandHandler<AssignTaskCommand, AssignTaskResponse>, AssignTaskCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteTaskCommand, CompleteTaskResponse>, CompleteTaskCommandHandler>();
        services.AddScoped<ICommandHandler<CancelTaskCommand, CancelTaskResponse>, CancelTaskCommandHandler>();
        services.AddScoped<ICommandHandler<RescheduleTaskCommand, RescheduleTaskResponse>, RescheduleTaskCommandHandler>();
        services.AddScoped<ICommandHandler<CreateRoutineCommand, CreateRoutineResponse>, CreateRoutineCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateRoutineCommand, UpdateRoutineResponse>, UpdateRoutineCommandHandler>();
        services.AddScoped<ICommandHandler<PauseRoutineCommand, PauseRoutineResponse>, PauseRoutineCommandHandler>();
        services.AddScoped<ICommandHandler<ResumeRoutineCommand, ResumeRoutineResponse>, ResumeRoutineCommandHandler>();
        services.AddScoped<ICommandHandler<ReassignTaskCommand, ReassignTaskResponse>, ReassignTaskCommandHandler>();
        services.AddScoped<IQueryHandler<GetRoutinesByFamilyQuery, RoutineListResponse>, GetRoutinesByFamilyQueryHandler>();

        // Calendar coordination slices (Phase 6)
        services.AddScoped<IQueryHandler<DetectCalendarConflictsQuery, CalendarConflictsResponse>, DetectCalendarConflictsQueryHandler>();
        services.AddScoped<IQueryHandler<SuggestEventParticipantsQuery, SuggestEventParticipantsResponse>, SuggestEventParticipantsQueryHandler>();
        services.AddScoped<IQueryHandler<ProposeAlternativeTimesQuery, ProposeAlternativeTimesResponse>, ProposeAlternativeTimesQueryHandler>();
        services.AddScoped<IQueryHandler<GetFamilyPlansQuery, FamilyPlansResponse>, GetFamilyPlansQueryHandler>();

        // Chore coordination slices (Phase 6)
        services.AddScoped<IQueryHandler<SuggestResponsibilityOwnerQuery, SuggestResponsibilityOwnerResponse>, SuggestResponsibilityOwnerQueryHandler>();
        services.AddScoped<IQueryHandler<GetResponsibilityBalanceQuery, ResponsibilityBalanceResponse>, GetResponsibilityBalanceQueryHandler>();
        services.AddScoped<IQueryHandler<DetectResponsibilityOverloadQuery, ResponsibilityOverloadResponse>, DetectResponsibilityOverloadQueryHandler>();

        // Household system slices (Phase 6)
        services.AddScoped<IQueryHandler<GetHouseholdAreasQuery, HouseholdAreasResponse>, GetHouseholdAreasQueryHandler>();
        services.AddScoped<IQueryHandler<GetResponsibilityVisibilityQuery, ResponsibilityVisibilityResponse>, GetResponsibilityVisibilityQueryHandler>();
        services.AddScoped<IQueryHandler<GetMemberActivityQuery, MemberActivityResponse>, GetMemberActivityQueryHandler>();

        // Timeline enrichment slices (Phase 6)
        services.AddScoped<IQueryHandler<GetEnrichedTimelineQuery, EnrichedTimelineResponse>, GetEnrichedTimelineQueryHandler>();
        services.AddScoped<IQueryHandler<GetWeeklyGridQuery, WeeklyGridResponse>, GetWeeklyGridQueryHandler>();

        // Languages
        services.AddScoped<IQueryHandler<GetSupportedLanguagesQuery, SupportedLanguagesResponse>, GetSupportedLanguagesQueryHandler>();

        // Setup slices
        services.AddScoped<IQueryHandler<GetSetupStatusQuery, SetupStatusResponse>, GetSetupStatusQueryHandler>();
        services.AddScoped<ICommandHandler<InitializeSystemCommand, InitializeSystemResponse>, InitializeSystemCommandHandler>();

        // Shared Lists slices (V1.1)
        services.AddScoped<ICommandHandler<CreateSharedListCommand, CreateSharedListResponse>, CreateSharedListCommandHandler>();
        services.AddScoped<ICommandHandler<AddItemToSharedListCommand, AddItemToSharedListResponse>, AddItemToSharedListCommandHandler>();
        services.AddScoped<ICommandHandler<ToggleSharedListItemCommand, ToggleSharedListItemResponse>, ToggleSharedListItemCommandHandler>();
        services.AddScoped<IQueryHandler<GetFamilySharedListsQuery, GetFamilySharedListsResponse>, GetFamilySharedListsQueryHandler>();
        services.AddScoped<IQueryHandler<GetSharedListDetailQuery, GetSharedListDetailResponse>, GetSharedListDetailQueryHandler>();
        services.AddScoped<ICommandHandler<UpdateSharedListItemCommand, UpdateSharedListItemResponse>, UpdateSharedListItemCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveSharedListItemCommand, bool>, RemoveSharedListItemCommandHandler>();
        services.AddScoped<ICommandHandler<LinkSharedListCommand, LinkSharedListResponse>, LinkSharedListCommandHandler>();
        services.AddScoped<ICommandHandler<UnlinkSharedListCommand, bool>, UnlinkSharedListCommandHandler>();
        services.AddScoped<ICommandHandler<CreateLinkedSharedListForEventCommand, CreateSharedListResponse>, CreateLinkedSharedListForEventCommandHandler>();
        services.AddScoped<IQueryHandler<GetSharedListByLinkedEntityQuery, GetSharedListByLinkedEntityResponse>, GetSharedListByLinkedEntityQueryHandler>();
        services.AddScoped<ICommandHandler<RenameSharedListCommand, RenameSharedListResponse>, RenameSharedListCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteSharedListCommand, bool>, DeleteSharedListCommandHandler>();
        services.AddScoped<ICommandHandler<ReorderSharedListItemsCommand, bool>, ReorderSharedListItemsCommandHandler>();

        return services;
    }
}
