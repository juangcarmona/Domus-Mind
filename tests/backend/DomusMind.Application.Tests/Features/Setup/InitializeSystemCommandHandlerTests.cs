using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Abstractions.System;
using DomusMind.Application.Features.Setup;
using DomusMind.Application.Features.Setup.InitializeSystem;
using FluentAssertions;

namespace DomusMind.Application.Tests.Features.Setup;

public sealed class InitializeSystemCommandHandlerTests
{
    private static InitializeSystemCommandHandler BuildHandler(
        InMemorySystemInitializationState? state = null,
        InMemoryAuthUserRepository? users = null,
        IPasswordHasher? hasher = null)
    {
        return new InitializeSystemCommandHandler(
            state ?? new InMemorySystemInitializationState(),
            users ?? new InMemoryAuthUserRepository(),
            hasher ?? new StubPasswordHasher());
    }

    [Fact]
    public async Task Handle_WhenNotInitialized_CreatesAdminUser()
    {
        var users = new InMemoryAuthUserRepository();
        var handler = BuildHandler(users: users);

        await handler.Handle(
            new InitializeSystemCommand("admin@example.com", "SecurePass1!", null),
            CancellationToken.None);

        users.Users.Should().HaveCount(1);
        users.Users[0].Email.Should().Be("admin@example.com");
    }

    [Fact]
    public async Task Handle_WhenNotInitialized_MarksSystemAsInitialized()
    {
        var state = new InMemorySystemInitializationState();
        var handler = BuildHandler(state: state);

        await handler.Handle(
            new InitializeSystemCommand("admin@example.com", "SecurePass1!", null),
            CancellationToken.None);

        state.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAlreadyInitialized_ThrowsAlreadyInitializedException()
    {
        var state = new InMemorySystemInitializationState(initialized: true);
        var handler = BuildHandler(state: state);

        var act = () => handler.Handle(
            new InitializeSystemCommand("admin@example.com", "SecurePass1!", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<SetupException>()
            .Where(e => e.Code == SetupErrorCode.AlreadyInitialized);
    }

    [Fact]
    public async Task Handle_WhenAlreadyInitialized_DoesNotCreateAnyUser()
    {
        var state = new InMemorySystemInitializationState(initialized: true);
        var users = new InMemoryAuthUserRepository();
        var handler = BuildHandler(state: state, users: users);

        try { await handler.Handle(new InitializeSystemCommand("admin@example.com", "SecurePass1!", null), CancellationToken.None); }
        catch (SetupException) { /* expected */ }

        users.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_HashesPasswordBeforeStoring()
    {
        var users = new InMemoryAuthUserRepository();
        var hasher = new RecordingPasswordHasher();
        var handler = BuildHandler(users: users, hasher: hasher);

        await handler.Handle(
            new InitializeSystemCommand("admin@example.com", "SecurePass1!", null),
            CancellationToken.None);

        hasher.LastHashed.Should().Be("SecurePass1!");
        users.Users[0].PasswordHash.Should().NotBe("SecurePass1!");
    }

    [Fact]
    public async Task Handle_WithWeakPassword_ThrowsWeakPasswordException()
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new InitializeSystemCommand("admin@example.com", "short", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<SetupException>()
            .Where(e => e.Code == SetupErrorCode.WeakPassword);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ThrowsEmailAlreadyTakenException()
    {
        var existingUser = new AuthUserRecord(Guid.NewGuid(), "admin@example.com", "hash");
        var users = new InMemoryAuthUserRepository([existingUser]);
        var handler = BuildHandler(users: users);

        var act = () => handler.Handle(
            new InitializeSystemCommand("admin@example.com", "SecurePass1!", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<SetupException>()
            .Where(e => e.Code == SetupErrorCode.EmailAlreadyTaken);
    }

    [Fact]
    public async Task Handle_NormalizesEmailToLowercase()
    {
        var users = new InMemoryAuthUserRepository();
        var handler = BuildHandler(users: users);

        await handler.Handle(
            new InitializeSystemCommand("Admin@EXAMPLE.COM", "SecurePass1!", null),
            CancellationToken.None);

        users.Users[0].Email.Should().Be("admin@example.com");
    }

    [Fact]
    public async Task Handle_ReturnsUserIdAndNormalizedEmail()
    {
        var handler = BuildHandler();

        var result = await handler.Handle(
            new InitializeSystemCommand("Admin@EXAMPLE.COM", "SecurePass1!", null),
            CancellationToken.None);

        result.Email.Should().Be("admin@example.com");
        result.UserId.Should().NotBe(Guid.Empty);
    }

    // ── Stubs ─────────────────────────────────────────────────────────────────

    private sealed class InMemorySystemInitializationState : ISystemInitializationState
    {
        public bool IsInitialized { get; private set; }

        public InMemorySystemInitializationState(bool initialized = false) => IsInitialized = initialized;

        public Task<bool> IsInitializedAsync(CancellationToken ct) => Task.FromResult(IsInitialized);

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
            => Users = seed?.ToList() ?? [];

        public Task<AuthUserRecord?> FindByEmailAsync(string email, CancellationToken ct)
            => Task.FromResult(Users.FirstOrDefault(u => u.Email == email));

        public Task<AuthUserRecord?> FindByIdAsync(Guid userId, CancellationToken ct)
            => Task.FromResult(Users.FirstOrDefault(u => u.UserId == userId));

        public Task AddAsync(AuthUserRecord user, CancellationToken ct) { Users.Add(user); return Task.CompletedTask; }
        public Task UpdatePasswordHashAsync(Guid userId, string hash, CancellationToken ct) => Task.CompletedTask;
        public Task UpdateMustChangePasswordAsync(Guid userId, bool mustChangePassword, CancellationToken ct) => Task.CompletedTask;
        public Task<bool> AnyUsersAsync(CancellationToken ct) => Task.FromResult(Users.Count > 0);
        public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class StubPasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"HASHED:{password}";
        public bool Verify(string password, string hash) => hash == $"HASHED:{password}";
    }

    private sealed class RecordingPasswordHasher : IPasswordHasher
    {
        public string? LastHashed { get; private set; }
        public string Hash(string password) { LastHashed = password; return $"HASHED:{password}"; }
        public bool Verify(string password, string hash) => hash == $"HASHED:{password}";
    }
}
