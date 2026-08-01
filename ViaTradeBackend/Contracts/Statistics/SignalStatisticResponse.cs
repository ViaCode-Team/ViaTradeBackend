using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Statistics;

public record SignalStatisticResponse(
	[Range(0, int.MaxValue)] int TotalSignals,
	[Range(0, int.MaxValue)] int BuySignals,
	[Range(0, int.MaxValue)] int SellSignals
);
