using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Configuration.Options;

public sealed class JwtSettings
{
	[Required]
	public string Issuer { get; set; } = string.Empty;

	[Required]
	public string Audience { get; set; } = string.Empty;

	[Required]
	[MinLength(32)]
	public string Secret { get; set; } = string.Empty;

	[Range(1, int.MaxValue)]
	public int AccessTokenMinutes { get; set; }
}
