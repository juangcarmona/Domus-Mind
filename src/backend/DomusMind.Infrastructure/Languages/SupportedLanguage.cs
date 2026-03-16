namespace DomusMind.Infrastructure.Languages;

/// <summary>
/// Reference data row for a language supported by the DomusMind product.
/// Not a domain entity — infrastructure-level lookup table.
/// </summary>
public sealed class SupportedLanguage
{
    /// <summary>BCP 47 language tag, e.g. "en", "fr", "zh".</summary>
    public string Code { get; init; } = default!;

    /// <summary>Full BCP 47 culture string, e.g. "en-US", "fr-FR".</summary>
    public string Culture { get; init; } = default!;

    /// <summary>English display name, e.g. "French".</summary>
    public string DisplayName { get; init; } = default!;

    /// <summary>Native display name, e.g. "Français".</summary>
    public string NativeDisplayName { get; init; } = default!;

    public bool IsDefault { get; init; }

    public bool IsActive { get; init; }

    public int SortOrder { get; init; }
}
