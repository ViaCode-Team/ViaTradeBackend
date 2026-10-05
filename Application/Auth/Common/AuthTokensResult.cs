namespace ViaTrade.Application.Auth.Common;

public sealed record AuthTokensResult
{
	public required string AccessToken { get; init; }

	public required string RefreshToken { get; init; }

	public required DateTimeOffset AccessTokenExpiresAt { get; init; }

	public required DateTimeOffset RefreshTokenExpiresAt { get; init; }
}
