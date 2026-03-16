namespace DomusMind.Application.Abstractions.Languages;

public sealed record SupportedLanguageItem(
    string Code,
    string Culture,
    string DisplayName,
    string NativeDisplayName,
    bool IsDefault,
    int SortOrder);
