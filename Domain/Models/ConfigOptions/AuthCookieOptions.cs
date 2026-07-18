namespace Domain.Models.ConfigOptions;

public class AuthCookieOptions
{
	public required string AccessTokenCookie { get; set; }

	public required string RefreshTokenCookie { get; set; }

	public int RefreshTokenExpiryDays { get; set; }
}
