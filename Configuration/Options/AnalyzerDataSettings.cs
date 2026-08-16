using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Configuration.Options;

public sealed class AnalyzerDataSettings
{
	[Required]
	public string SourcePath { get; set; } = string.Empty;

	[Required]
	public string FuturesDataDirectoryName { get; set; } = string.Empty;

	[Required]
	public string StocksDataDirectoryName { get; set; } = string.Empty;

	[Required]
	public string StrategyResultDirectoryName { get; set; } = string.Empty;

	[Required]
	public string ScrennerResultDirectoryName { get; set; } = string.Empty;
}
