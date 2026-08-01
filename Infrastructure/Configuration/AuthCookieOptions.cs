namespace Infrastructure.Configuration;

public class AuthCookieOptions
{
	public required string AccessTokenCookie { get; set; }

	public required string RefreshTokenCookie { get; set; }

	public int RefreshTokenExpiryDays { get; set; }

	public int AbsoluteSessionLifetimeDays { get; set; } = 30;
}
