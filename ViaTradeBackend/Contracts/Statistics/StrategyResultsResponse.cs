namespace ViaTradeBackend.Contracts.Statistics;

public record StrategyResultsResponse(List<StrategyDataResponse> Strategies)
{
	public StrategyResultsResponse()
		: this([]) { }
}

public record StrategyDataResponse(string Name, List<TickerResultsResponse> Tickers);

public record TickerResultsResponse(string TradeCode, int? Accuracy, List<StrategyResultResponse> Results);

public record StrategyResultResponse(DateTime Date, decimal ClosePrice, string Signal);
