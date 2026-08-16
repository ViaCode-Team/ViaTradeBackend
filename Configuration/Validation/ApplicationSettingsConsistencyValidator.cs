using Microsoft.Extensions.Options;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Configuration.Validation;

public sealed class ApplicationSettingsConsistencyValidator : IValidateOptions<ApplicationSettings>
{
	public ValidateOptionsResult Validate(string? name, ApplicationSettings settings)
	{
		var failures = new List<string>();

		if (settings.AuthCookies.AccessTokenCookie == settings.AuthCookies.RefreshTokenCookie)
			failures.Add("Authentication cookie names must be different.");

		if (settings.AuthCookies.RefreshTokenExpiryDays > settings.AuthCookies.AbsoluteSessionLifetimeDays)
			failures.Add("AuthCookies:RefreshTokenExpiryDays cannot exceed AuthCookies:AbsoluteSessionLifetimeDays.");

		if (failures.Count == 0)
			return ValidateOptionsResult.Success;

		return ValidateOptionsResult.Fail(failures);
	}
}
