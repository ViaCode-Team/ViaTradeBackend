using Microsoft.Extensions.Options;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Configuration.Validation;

public sealed class ApplicationSettingsConsistencyValidator : IValidateOptions<ApplicationSettings>
{
	public ValidateOptionsResult Validate(string? name, ApplicationSettings settings)
	{
		var failures = new List<string>();

		var validFrontend = Uri.TryCreate(settings.Email.FrontendUrl, UriKind.Absolute, out var frontend);
		if (
			validFrontend
			&& frontend != null
			&& frontend.Scheme != Uri.UriSchemeHttps
			&& (frontend.Scheme != Uri.UriSchemeHttp || !frontend.IsLoopback)
		)
			failures.Add("Email:FrontendUrl must use HTTPS outside loopback development.");
		if (settings.Email.AllowedOrigins.Any(origin => origin.Contains('*')))
			failures.Add("Email:AllowedOrigins must contain explicit origins without wildcards.");
		if (settings.Email.RetryBaseSeconds > settings.Email.RetryMaxSeconds)
			failures.Add("Email:RetryBaseSeconds cannot exceed RetryMaxSeconds.");

		if (settings.AuthCookies.AccessTokenCookie == settings.AuthCookies.RefreshTokenCookie)
			failures.Add("Authentication cookie names must be different.");

		if (settings.AuthCookies.RefreshTokenExpiryDays > settings.AuthCookies.AbsoluteSessionLifetimeDays)
			failures.Add("AuthCookies:RefreshTokenExpiryDays cannot exceed AuthCookies:AbsoluteSessionLifetimeDays.");

		if (failures.Count == 0)
			return ValidateOptionsResult.Success;

		return ValidateOptionsResult.Fail(failures);
	}
}
