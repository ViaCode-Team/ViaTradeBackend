using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Configuration.Options;

public sealed class ServiceSecuritySettings
{
	[Required]
	[MinLength(32)]
	public string Password { get; set; } = string.Empty;
}
