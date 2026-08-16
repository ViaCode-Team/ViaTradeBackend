using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Api.Contracts.Signals;

public record SignalResponse(
	[Range(1, int.MaxValue)] int StrategyId,
	[StringLength(255)] string StrategyName,
	[StringLength(255)] string DisplayName,
	[Range(1, int.MaxValue)] int InstrumentId,
	[StringLength(255)] string Symbol,
	[Range(0, 100)] int? Accuracy,
	DateTime Date,
	decimal ClosePrice,
	[StringLength(16)] string Signal
);
