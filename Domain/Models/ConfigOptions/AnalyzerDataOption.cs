namespace Domain.Models.ConfigOptions;

public class AnalyzerDataOption
{
	public required string SourcePath { get; set; }
	public required string FuturesDataDirectoryName { get; set; }
	public required string StocksDataDirectoryName { get; set; }
	public required string StrategyResultDirectoryName { get; set; }
	public required string ScrennerResultDirectoryName { get; set; }

}
