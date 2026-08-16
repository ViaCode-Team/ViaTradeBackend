using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Configuration.Options;

public sealed class AuthCookieSettings
{
	[Required]
	public string AccessTokenCookie { get; set; } = string.Empty;

	[Required]
	public string RefreshTokenCookie { get; set; } = string.Empty;

	[Range(1, int.MaxValue)]
	public int RefreshTokenExpiryDays { get; set; }

	[Range(1, int.MaxValue)]
	public int AbsoluteSessionLifetimeDays { get; set; }
}
