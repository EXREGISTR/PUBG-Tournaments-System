using FluentValidation;

namespace Domain.Common.Extensions;

internal static class UrlValidator {
    public static IRuleBuilderOptionsConditions<T, string?> CorrectHostAndScheme<T>(this IRuleBuilder<T, string?> builder) =>
        builder.Custom((url, context) => {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? result)) {
                context.AddFailure("Invalid url");
                return;
            }
            
            if (result.Scheme != Uri.UriSchemeHttp && result.Scheme != Uri.UriSchemeHttps) {
                context.AddFailure($"Invalid scheme: {result.Scheme}");
            }

            if (result.Host != "127.0.0.1" && result.Port != 8000) {
                context.AddFailure($"Unknown address: {result.Host}:{result.Port}");
            }
        });
}