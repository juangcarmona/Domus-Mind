namespace DomusMind.Contracts.Languages;

public sealed record SupportedLanguageItem(
    string Code,
    string Culture,
    string DisplayName,
    string NativeDisplayName,
    bool IsDefault,
    int SortOrder);

public sealed record SupportedLanguagesResponse(
    IReadOnlyCollection<SupportedLanguageItem> Languages);
