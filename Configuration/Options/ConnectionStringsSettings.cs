using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Configuration.Options;

public sealed class ConnectionStringsSettings
{
	[Required]
	public string MySql { get; set; } = string.Empty;

	[Required]
	public string Redis { get; set; } = string.Empty;
}
