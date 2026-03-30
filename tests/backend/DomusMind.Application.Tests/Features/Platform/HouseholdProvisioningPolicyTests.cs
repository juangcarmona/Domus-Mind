using DomusMind.Application.Abstractions.Platform;
using DomusMind.Infrastructure.Persistence;
using DomusMind.Infrastructure.Platform;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DomusMind.Application.Tests.Features.Platform;

public sealed class HouseholdProvisioningPolicyTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static HouseholdProvisioningPolicy BuildPolicy(
        DomusMindDbContext db,
        IDeploymentModeContext context)
        => new(context, db, NullLogger<HouseholdProvisioningPolicy>.Instance);

    private static StubDeploymentModeContext SingleInstance(
        bool allowCreation = true,
        bool requireInvitationForSignup = false)
        => new(DeploymentMode.SingleInstance, allowCreation, requireInvitationForSignup: requireInvitationForSignup);

    private static StubDeploymentModeContext CloudHosted(
        bool allowCreation = true,
        bool invitationsEnabled = false,
        bool requireInvitationForSignup = false,
        int maxHouseholds = 0)
        => new(DeploymentMode.CloudHosted, allowCreation, invitationsEnabled: invitationsEnabled, requireInvitationForSignup: requireInvitationForSignup, maxHouseholdsPerDeployment: maxHouseholds);

    [Fact]
    public async Task SingleInstance_NoHouseholds_Permits()
    {
        var db = CreateDb();
        var policy = BuildPolicy(db, SingleInstance());

        var result = await policy.EvaluateAsync(CancellationToken.None);

        result.Allowed.Should().BeTrue();
        result.ReasonCode.Should().Be("allowed");
    }

    [Fact]
    public async Task SingleInstance_OneHouseholdExists_DeniesWithSingleInstanceBound()
    {
        var db = CreateDb();
        var family = Domain.Family.Family.Create(
            Domain.Family.FamilyId.New(),
            Domain.Family.ValueObjects.FamilyName.Create("Test Family"),
            null,
            DateTime.UtcNow);
        db.Families.Add(family);
        await db.SaveChangesAsync();

        var policy = BuildPolicy(db, SingleInstance());

        var result = await policy.EvaluateAsync(CancellationToken.None);

        result.Allowed.Should().BeFalse();
        result.ReasonCode.Should().Be("single_instance_already_bound");
    }

    [Fact]
    public async Task SingleInstance_CreationDisabled_Denies()
    {
        var db = CreateDb();
        var policy = BuildPolicy(db, SingleInstance(allowCreation: false));

        var result = await policy.EvaluateAsync(CancellationToken.None);

        result.Allowed.Should().BeFalse();
        result.ReasonCode.Should().Be("household_creation_disabled");
    }

    [Fact]
    public async Task CloudHosted_NoHouseholds_Permits()
    {
        var db = CreateDb();
        var policy = BuildPolicy(db, CloudHosted());

        var result = await policy.EvaluateAsync(CancellationToken.None);

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task CloudHosted_RequireInvitationForSignup_DeniesWithInvitationRequired()
    {
        var db = CreateDb();
        var policy = BuildPolicy(db, CloudHosted(requireInvitationForSignup: true));

        var result = await policy.EvaluateAsync(CancellationToken.None);

        result.Allowed.Should().BeFalse();
        result.ReasonCode.Should().Be("invitation_required");
    }

    [Fact]
    public async Task CloudHosted_InvitationsEnabled_WithoutRequireInvitation_Permits()
    {
        var db = CreateDb();
        var policy = BuildPolicy(db, CloudHosted(invitationsEnabled: true, requireInvitationForSignup: false));

        var result = await policy.EvaluateAsync(CancellationToken.None);

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task CloudHosted_MaxHouseholdsNotReached_Permits()
    {
        var db = CreateDb();
        var policy = BuildPolicy(db, CloudHosted(maxHouseholds: 5));

        var result = await policy.EvaluateAsync(CancellationToken.None);

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task CloudHosted_MaxHouseholdsReached_DeniesWithMaxHouseholdsReached()
    {
        var db = CreateDb();
        // Seed 3 families
        for (var i = 0; i < 3; i++)
        {
            var family = Domain.Family.Family.Create(
                Domain.Family.FamilyId.New(),
                Domain.Family.ValueObjects.FamilyName.Create($"Family {i}"),
                null,
                DateTime.UtcNow);
            db.Families.Add(family);
        }
        await db.SaveChangesAsync();

        // Cap = 3 → already at limit
        var policy = BuildPolicy(db, CloudHosted(maxHouseholds: 3));

        var result = await policy.EvaluateAsync(CancellationToken.None);

        result.Allowed.Should().BeFalse();
        result.ReasonCode.Should().Be("max_households_reached");
    }

    [Fact]
    public async Task CloudHosted_ZeroMaxHouseholds_AlwaysPermits()
    {
        var db = CreateDb();
        // Seed many families
        for (var i = 0; i < 10; i++)
        {
            var family = Domain.Family.Family.Create(
                Domain.Family.FamilyId.New(),
                Domain.Family.ValueObjects.FamilyName.Create($"Family {i}"),
                null,
                DateTime.UtcNow);
            db.Families.Add(family);
        }
        await db.SaveChangesAsync();

        // 0 = unlimited
        var policy = BuildPolicy(db, CloudHosted(maxHouseholds: 0));

        var result = await policy.EvaluateAsync(CancellationToken.None);

        result.Allowed.Should().BeTrue();
    }

    // ── Stub ─────────────────────────────────────────────────────────────────

    private sealed class StubDeploymentModeContext : IDeploymentModeContext
    {
        public StubDeploymentModeContext(
            DeploymentMode mode,
            bool canCreateHousehold = true,
            bool invitationsEnabled = false,
            bool requireInvitationForSignup = false,
            int maxHouseholdsPerDeployment = 0)
        {
            Mode = mode;
            CanCreateHousehold = canCreateHousehold;
            InvitationsEnabled = invitationsEnabled;
            RequireInvitationForSignup = requireInvitationForSignup;
            MaxHouseholdsPerDeployment = maxHouseholdsPerDeployment;
        }

        public DeploymentMode Mode { get; }
        public bool CanCreateHousehold { get; }
        public bool InvitationsEnabled { get; }
        public bool RequireInvitationForSignup { get; }
        public bool EmailEnabled => false;
        public bool SupportsAdminTools => false;
        public int MaxHouseholdsPerDeployment { get; }
    }
}
