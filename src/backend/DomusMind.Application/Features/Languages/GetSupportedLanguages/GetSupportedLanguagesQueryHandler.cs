using DomusMind.Application.Abstractions.Languages;
using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Languages;

namespace DomusMind.Application.Features.Languages.GetSupportedLanguages;

public sealed class GetSupportedLanguagesQueryHandler
    : IQueryHandler<GetSupportedLanguagesQuery, SupportedLanguagesResponse>
{
    private readonly ISupportedLanguageReader _languageReader;

    public GetSupportedLanguagesQueryHandler(ISupportedLanguageReader languageReader)
    {
        _languageReader = languageReader;
    }

    public async Task<SupportedLanguagesResponse> Handle(
        GetSupportedLanguagesQuery query,
        CancellationToken cancellationToken)
    {
        var items = await _languageReader.GetActiveAsync(cancellationToken);

        var response = items.Select(l => new Contracts.Languages.SupportedLanguageItem(
            l.Code,
            l.Culture,
            l.DisplayName,
            l.NativeDisplayName,
            l.IsDefault,
            l.SortOrder)).ToList();

        return new SupportedLanguagesResponse(response.AsReadOnly());
    }
}
