using DomusMind.Application.Abstractions.Languages;
using DomusMind.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Infrastructure.Languages;

public sealed class SupportedLanguageReader : ISupportedLanguageReader
{
    private readonly DomusMindDbContext _db;

    public SupportedLanguageReader(DomusMindDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<SupportedLanguageItem>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.SupportedLanguages
            .AsNoTracking()
            .Where(l => l.IsActive)
            .OrderBy(l => l.SortOrder)
            .Select(l => new SupportedLanguageItem(
                l.Code,
                l.Culture,
                l.DisplayName,
                l.NativeDisplayName,
                l.IsDefault,
                l.SortOrder))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsActiveAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _db.SupportedLanguages
            .AsNoTracking()
            .AnyAsync(l => l.IsActive && l.Code == code, cancellationToken);
    }
}
