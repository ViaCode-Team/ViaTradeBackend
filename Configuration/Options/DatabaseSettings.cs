using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Configuration.Options;

public sealed class DatabaseSettings
{
	[Required]
	public int MaxRetryCount { get; set; } = 3;

	[Required]
	public int MaxRetryDelaySeconds { get; set; } = 5;
}
