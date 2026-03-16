using DomusMind.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DomusMind.Infrastructure.Languages;

/// <summary>
/// Seeds the supported languages reference table on first startup.
/// Must be called after migrations have run.
/// </summary>
public sealed class LanguageSeedService
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<DomusMindDbContext>();
        var logger = sp.GetRequiredService<ILogger<LanguageSeedService>>();

        if (await db.SupportedLanguages.AnyAsync(cancellationToken))
        {
            logger.LogDebug("Language seed skipped: languages already exist.");
            return;
        }

        var languages = new[]
        {
            new SupportedLanguage { Code = "en", Culture = "en-US", DisplayName = "English",  NativeDisplayName = "English",    IsDefault = true,  IsActive = true, SortOrder = 0 },
            new SupportedLanguage { Code = "de", Culture = "de-DE", DisplayName = "German",   NativeDisplayName = "Deutsch",     IsDefault = false, IsActive = true, SortOrder = 1 },
            new SupportedLanguage { Code = "es", Culture = "es-ES", DisplayName = "Spanish",  NativeDisplayName = "Español",     IsDefault = false, IsActive = true, SortOrder = 2 },
            new SupportedLanguage { Code = "fr", Culture = "fr-FR", DisplayName = "French",   NativeDisplayName = "Français",    IsDefault = false, IsActive = true, SortOrder = 3 },
            new SupportedLanguage { Code = "it", Culture = "it-IT", DisplayName = "Italian",  NativeDisplayName = "Italiano",    IsDefault = false, IsActive = true, SortOrder = 4 },
            new SupportedLanguage { Code = "ja", Culture = "ja-JP", DisplayName = "Japanese", NativeDisplayName = "日本語",      IsDefault = false, IsActive = true, SortOrder = 5 },
            new SupportedLanguage { Code = "zh", Culture = "zh-CN", DisplayName = "Chinese",  NativeDisplayName = "中文",        IsDefault = false, IsActive = true, SortOrder = 6 },
        };

        db.SupportedLanguages.AddRange(languages);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded {Count} supported languages.", languages.Length);
    }
}
