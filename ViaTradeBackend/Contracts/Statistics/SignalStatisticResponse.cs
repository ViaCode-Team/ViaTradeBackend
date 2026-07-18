namespace ViaTradeBackend.Contracts.Statistics;

public record SignalStatisticResponse(
	int TotalSignals,
	int BuySignals,
	int SellSignals
);

