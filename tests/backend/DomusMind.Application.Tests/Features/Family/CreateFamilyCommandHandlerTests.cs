using DomusMind.Application.Abstractions.Languages;
using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.CreateFamily;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Family;

public sealed class CreateFamilyCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static CreateFamilyCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubFamilyAccessGranter? granter = null,
        StubSupportedLanguageReader? languageReader = null)
    {
        var context = db ?? CreateDb();
        return new CreateFamilyCommandHandler(
            context,
            new EventLogWriter(context),
            granter ?? new StubFamilyAccessGranter(),
            languageReader ?? new StubSupportedLanguageReader());
    }

    [Fact]
    public async Task Handle_WithValidName_ReturnsFamilyResponse()
    {
        var userId = Guid.NewGuid();
        var handler = BuildHandler();

        var result = await handler.Handle(
            new CreateFamilyCommand("Smith Family", null, userId),
            CancellationToken.None);

        result.FamilyId.Should().NotBeEmpty();
        result.Name.Should().Be("Smith Family");
        result.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_WithValidLanguageCode_PersistsLanguage()
    {
        var userId = Guid.NewGuid();
        var reader = new StubSupportedLanguageReader(supportedCodes: ["en", "fr"]);
        var handler = BuildHandler(languageReader: reader);

        var result = await handler.Handle(
            new CreateFamilyCommand("Dupont Family", "fr", userId),
            CancellationToken.None);

        result.PrimaryLanguageCode.Should().Be("fr");
    }

    [Fact]
    public async Task Handle_WithUnsupportedLanguageCode_ThrowsFamilyException()
    {
        var handler = BuildHandler(languageReader: new StubSupportedLanguageReader(supportedCodes: ["en"]));

        var act = () => handler.Handle(
            new CreateFamilyCommand("Smith Family", "xx", Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_WithNullLanguageCode_PersistsNullLanguage()
    {
        var db = CreateDb();
        var handler = BuildHandler(db: db);
        var userId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateFamilyCommand("Davis Family", null, userId),
            CancellationToken.None);

        result.PrimaryLanguageCode.Should().BeNull();
    }

    [Fact]
    public async Task Handle_GrantsAccessToRequestingUser()
    {
        var userId = Guid.NewGuid();
        var granter = new StubFamilyAccessGranter();
        var handler = BuildHandler(granter: granter);

        var result = await handler.Handle(
            new CreateFamilyCommand("Johnson Family", null, userId),
            CancellationToken.None);

        granter.GrantedAccesses.Should().ContainSingle();
        var (grantedUserId, grantedFamilyId) = granter.GrantedAccesses.Single();
        grantedUserId.Should().Be(userId);
        grantedFamilyId.Should().Be(result.FamilyId);
    }

    [Fact]
    public async Task Handle_PersistsFamilyToDatabase()
    {
        var db = CreateDb();
        var handler = BuildHandler(db: db);
        var userId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateFamilyCommand("Davis Family", null, userId),
            CancellationToken.None);

        var saved = await db.Families.FindAsync(
            Domain.Family.FamilyId.From(result.FamilyId));
        saved.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithEmptyName_ThrowsFamilyException(string name)
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new CreateFamilyCommand(name, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_WithNameTooLong_ThrowsFamilyException()
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new CreateFamilyCommand(new string('X', 101), null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.InvalidInput);
    }
}
