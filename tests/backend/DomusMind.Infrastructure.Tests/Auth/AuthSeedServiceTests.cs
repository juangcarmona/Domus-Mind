using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Abstractions.System;
using DomusMind.Infrastructure.Auth;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DomusMind.Infrastructure.Tests.Auth;

public sealed class AuthSeedServiceTests
{
    private static IServiceProvider BuildServices(
        BootstrapAdminOptions options,
        InMemoryAuthUserRepository repo,
        InMemorySystemInitializationState? initState = null,
        IPasswordHasher? hasher = null)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IAuthUserRepository>(repo);
        services.AddSingleton<IPasswordHasher>(hasher ?? new PasswordHasher());
        services.AddSingleton<ISystemInitializationState>(initState ?? new InMemorySystemInitializationState());
        services.AddSingleton(Options.Create(options));
        services.AddLogging();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task SeedAdminAsync_WhenEnabledAndNotInitialized_CreatesAdmin()
    {
        var repo = new InMemoryAuthUserRepository();
        var options = new BootstrapAdminOptions
        {
            Enabled = true,
            Email = "admin@domusmind.local",
            Password = "SecureAdmin1!",
            DisplayName = "Test Admin",
        };

        await AuthSeedService.SeedAdminAsync(BuildServices(options, repo), CancellationToken.None);

        repo.Users.Should().HaveCount(1);
        repo.Users[0].Email.Should().Be("admin@domusmind.local");
    }

    [Fact]
    public async Task SeedAdminAsync_WhenEnabledAndNotInitialized_MarksSystemAsInitialized()
    {
        var repo = new InMemoryAuthUserRepository();
        var initState = new InMemorySystemInitializationState();
        var options = new BootstrapAdminOptions
        {
            Enabled = true,
            Email = "admin@domusmind.local",
            Password = "SecureAdmin1!",
        };

        await AuthSeedService.SeedAdminAsync(BuildServices(options, repo, initState), CancellationToken.None);

        initState.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public async Task SeedAdminAsync_WhenDisabled_DoesNotCreateAnyUser()
    {
        var repo = new InMemoryAuthUserRepository();
        var options = new BootstrapAdminOptions { Enabled = false };

        await AuthSeedService.SeedAdminAsync(BuildServices(options, repo), CancellationToken.None);

        repo.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task SeedAdminAsync_WhenAlreadyInitialized_SkipsEvenIfBootstrapEnabled()
    {
        var repo = new InMemoryAuthUserRepository();
        var initState = new InMemorySystemInitializationState(alreadyInitialized: true);
        var options = new BootstrapAdminOptions
        {
            Enabled = true,
            Email = "admin@domusmind.local",
            Password = "SecureAdmin1!",
        };

        await AuthSeedService.SeedAdminAsync(BuildServices(options, repo, initState), CancellationToken.None);

        repo.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task SeedAdminAsync_HashesPasswordBeforeStoring()
    {
        var repo = new InMemoryAuthUserRepository();
        var hasher = new RecordingPasswordHasher();
        var options = new BootstrapAdminOptions
        {
            Enabled = true,
            Email = "admin@domusmind.local",
            Password = "SecureAdmin1!",
        };

        await AuthSeedService.SeedAdminAsync(BuildServices(options, repo, hasher: hasher), CancellationToken.None);

        hasher.LastHashed.Should().Be("SecureAdmin1!");
        repo.Users[0].PasswordHash.Should().NotBe("SecureAdmin1!");
    }

    [Fact]
    public async Task SeedAdminAsync_IsIdempotent_SecondCallIsNoOp()
    {
        var repo = new InMemoryAuthUserRepository();
        var options = new BootstrapAdminOptions
        {
            Enabled = true,
            Email = "admin@domusmind.local",
            Password = "SecureAdmin1!",
        };
        var sp = BuildServices(options, repo);

        await AuthSeedService.SeedAdminAsync(sp, CancellationToken.None);
        await AuthSeedService.SeedAdminAsync(sp, CancellationToken.None);

        repo.Users.Should().HaveCount(1);
    }

    [Fact]
    public async Task SeedAdminAsync_WhenEnabledAndNotInitialized_CreatedUserHasOperatorFlag()
    {
        var repo = new InMemoryAuthUserRepository();
        var options = new BootstrapAdminOptions
        {
            Enabled = true,
            Email = "admin@domusmind.local",
            Password = "SecureAdmin1!",
        };

        await AuthSeedService.SeedAdminAsync(BuildServices(options, repo), CancellationToken.None);

        repo.Users.Should().HaveCount(1);
        repo.Users[0].IsOperator.Should().BeTrue();
    }

    [Fact]
    public async Task SeedAdminAsync_WhenUserAlreadyExistsButNotInitialized_MarksInitializedWithoutDuplicate()
    {
        var existingUser = new AuthUserRecord(Guid.NewGuid(), "admin@domusmind.local", "HASHED:pass", IsOperator: true);
        var repo = new InMemoryAuthUserRepository([existingUser]);
        var initState = new InMemorySystemInitializationState(alreadyInitialized: false);
        var options = new BootstrapAdminOptions
        {
            Enabled = true,
            Email = "admin@domusmind.local",
            Password = "SecureAdmin1!",
        };

        await AuthSeedService.SeedAdminAsync(BuildServices(options, repo, initState), CancellationToken.None);

        repo.Users.Should().HaveCount(1, "no duplicate should be created");
        initState.IsInitialized.Should().BeTrue("state should be reconciled");
    }

    // ── Test helpers ──────────────────────────────────────────────────────────

    private sealed class InMemorySystemInitializationState : ISystemInitializationState
    {
        public bool IsInitialized { get; private set; }

        public InMemorySystemInitializationState(bool alreadyInitialized = false)
        {
            IsInitialized = alreadyInitialized;
        }

        public Task<bool> IsInitializedAsync(CancellationToken ct)
            => Task.FromResult(IsInitialized);

        public Task MarkInitializedAsync(CancellationToken ct)
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryAuthUserRepository : IAuthUserRepository
    {
        public readonly List<AuthUserRecord> Users;

        public InMemoryAuthUserRepository(IEnumerable<AuthUserRecord>? seed = null)
        {
            Users = seed?.ToList() ?? [];
        }

        public Task<AuthUserRecord?> FindByEmailAsync(string email, CancellationToken ct)
            => Task.FromResult(Users.FirstOrDefault(u => u.Email == email));

        public Task<AuthUserRecord?> FindByIdAsync(Guid userId, CancellationToken ct)
            => Task.FromResult(Users.FirstOrDefault(u => u.UserId == userId));

        public Task AddAsync(AuthUserRecord user, CancellationToken ct)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdatePasswordHashAsync(Guid userId, string hash, CancellationToken ct)
            => Task.CompletedTask;

        public Task UpdateMustChangePasswordAsync(Guid userId, bool mustChangePassword, CancellationToken ct)
            => Task.CompletedTask;

        public Task UpdatePasswordChangedAtAsync(Guid userId, DateTime changedAt, CancellationToken ct)
            => Task.CompletedTask;

        public Task DisableUserAsync(Guid userId, CancellationToken ct)
            => Task.CompletedTask;

        public Task EnableUserAsync(Guid userId, CancellationToken ct)
            => Task.CompletedTask;

        public Task UpdateLastLoginAtAsync(Guid userId, DateTime lastLoginAtUtc, CancellationToken ct)
            => Task.CompletedTask;

        public Task<IReadOnlyDictionary<Guid, AuthUserStatusProjection>> GetStatusByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct)
            => Task.FromResult<IReadOnlyDictionary<Guid, AuthUserStatusProjection>>(new Dictionary<Guid, AuthUserStatusProjection>());

        public Task<bool> AnyUsersAsync(CancellationToken ct)
            => Task.FromResult(Users.Count > 0);

        public Task<int> CountAsync(CancellationToken ct)
            => Task.FromResult(Users.Count);

        public Task<IReadOnlyList<AdminAuthUserProjection>> GetAdminProjectionsAsync(string? search, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<AdminAuthUserProjection>>([]);

        public Task SaveChangesAsync(CancellationToken ct)
            => Task.CompletedTask;
    }

    private sealed class RecordingPasswordHasher : IPasswordHasher
    {
        public string? LastHashed { get; private set; }

        public string Hash(string password)
        {
            LastHashed = password;
            return $"HASHED:{password}";
        }

        public bool Verify(string password, string hash) => hash == $"HASHED:{password}";
    }
}
