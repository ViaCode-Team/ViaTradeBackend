using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Api.OptionsSetup;

public class JwtBearerOptionsSetup(IOptions<JwtSettings> jwtOptions, IOptions<AuthCookieSettings> cookieOptions)
	: IConfigureNamedOptions<JwtBearerOptions>
{
	private readonly JwtSettings _jwt = jwtOptions.Value;
	private readonly AuthCookieSettings _authCookie = cookieOptions.Value;

	public void Configure(JwtBearerOptions options)
	{
		Configure(null, options);
	}

	public void Configure(string? name, JwtBearerOptions options)
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = _jwt.Issuer,
			ValidAudience = _jwt.Audience,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret)),
			ClockSkew = TimeSpan.Zero,
		};

		options.Events = new JwtBearerEvents
		{
			OnMessageReceived = context =>
			{
				var hasToken = context.Request.Cookies.TryGetValue(_authCookie.AccessTokenCookie, out var token);

				if (hasToken)
					context.Token = token;

				return Task.CompletedTask;
			},
		};
	}
}
