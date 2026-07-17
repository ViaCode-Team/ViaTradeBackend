using Domain.Entities.CSV;

namespace Domain.Services;

public static class SignalStatisticsCalcService
{
	public static int CountTotalSignals(IEnumerable<StrategyResult> results)
	{
		return results.Count();
	}

	public static int CountBuySignals(IEnumerable<StrategyResult> results)
	{
		return results.Count(r => r.Signal == "BUY");
	}

	public static int CountSellSignals(IEnumerable<StrategyResult> results)
	{
		return results.Count(r => r.Signal == "SELL");
	}
}
