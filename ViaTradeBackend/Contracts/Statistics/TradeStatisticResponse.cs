using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Statistics;

public record TradeStatisticResponse(
	[Range(0, int.MaxValue)] int TotalTrades,
	[Range(0, int.MaxValue)] int WinTrades,
	[Range(0, int.MaxValue)] int LoseTrades
);
