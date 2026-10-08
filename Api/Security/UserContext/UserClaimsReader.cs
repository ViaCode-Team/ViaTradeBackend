using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ViaTrade.Application.Common.Exceptions;

namespace ViaTrade.Api.Security.UserContext;

internal static class UserClaimsReader
{
	public static int? FindUserId(ClaimsPrincipal? principal)
	{
		var identity = FindAuthenticatedIdentity(principal);
		if (identity == null)
			return null;

		var value = GetClaimValue(identity, ClaimTypes.NameIdentifier);
		var parsed = int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var userId);
		if (!parsed || userId <= 0)
			throw new InvalidTokenException("Access token does not contain a valid user identifier.");

		return userId;
	}

	public static string? FindSessionId(ClaimsPrincipal? principal)
	{
		var identity = FindAuthenticatedIdentity(principal);
		if (identity == null)
			return null;

		return GetClaimValue(identity, JwtRegisteredClaimNames.Jti);
	}

	private static ClaimsIdentity? FindAuthenticatedIdentity(ClaimsPrincipal? principal)
	{
		if (principal == null)
			return null;

		var identities = principal.Identities.Where(identity => identity.IsAuthenticated).ToArray();
		if (identities.Length == 0)
			return null;

		if (identities.Length != 1)
			throw new InvalidTokenException("Access token contains ambiguous user identities.");

		return identities[0];
	}

	private static string GetClaimValue(ClaimsIdentity identity, string type)
	{
		var claims = identity.FindAll(type).ToArray();
		if (claims.Length != 1 || string.IsNullOrWhiteSpace(claims[0].Value))
			throw new InvalidTokenException("Access token contains a missing or ambiguous identifier.");

		return claims[0].Value;
	}
}
