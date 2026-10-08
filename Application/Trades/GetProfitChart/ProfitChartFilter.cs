namespace ViaTrade.Application.Trades.GetProfitChart;

public sealed class ProfitChartFilter
{
	public DateOnly? StartDate { get; set; }

	public DateOnly? EndDate { get; set; }

	public ProfitChartGranularity Granularity { get; set; } = ProfitChartGranularity.Day;
}
