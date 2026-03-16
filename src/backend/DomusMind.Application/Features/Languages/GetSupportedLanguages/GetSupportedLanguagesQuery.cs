using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Languages;

namespace DomusMind.Application.Features.Languages.GetSupportedLanguages;

public sealed record GetSupportedLanguagesQuery : IQuery<SupportedLanguagesResponse>;
