using System.Text;
using Domain.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ViaTradeBackend.OptionsSetup
{
    public class JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions)
                : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly JwtOptions _jwt = jwtOptions.Value;

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
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_jwt.Secret)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.TryGetValue("access_token", out var token))
                        context.Token = token;

                    return Task.CompletedTask;
                }
            };
        }
    }

}
