using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Auth.Interfaces;
using Application.Users.Models;
using Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Utils;

public class JwtHelper(IOptions<JwtOptions> options) : IJwtHelper
{
	private readonly JwtOptions _options = options.Value;

	public string GenerateAccessToken(UserTokenDto user, string sessionId)
	{
		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new Claim(JwtRegisteredClaimNames.Jti, sessionId),
			new Claim(JwtRegisteredClaimNames.UniqueName, user.Login),
		};

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _options.Issuer,
			audience: _options.Audience,
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
			signingCredentials: creds
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	public string GenerateRefreshToken()
	{
		var bytes = RandomNumberGenerator.GetBytes(64);
		return Convert.ToBase64String(bytes);
	}

	public string GetSessionId(ClaimsPrincipal user)
	{
		var jtiClaim = user.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
		if (jtiClaim == null)
			throw new InvalidOperationException("User claims do not contain 'jti'.");

		return jtiClaim.Value;
	}

	public int GetUserIdFromClaims(ClaimsPrincipal user)
	{
		var subClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (subClaim == null)
			throw new InvalidOperationException("User claims do not contain 'sub'.");

		return int.Parse(subClaim);
	}
}
