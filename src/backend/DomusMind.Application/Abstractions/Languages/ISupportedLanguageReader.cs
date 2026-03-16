namespace DomusMind.Application.Abstractions.Languages;

public interface ISupportedLanguageReader
{
    Task<IReadOnlyCollection<SupportedLanguageItem>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<bool> IsActiveAsync(string code, CancellationToken cancellationToken = default);
}
