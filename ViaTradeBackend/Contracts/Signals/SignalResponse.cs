using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Signals;

public record SignalResponse(
	[Range(1, int.MaxValue)] int StrategyId,
	[StringLength(255)] string StrategyName,
	[Range(1, int.MaxValue)] int InstrumentId,
	[StringLength(255)] string Symbol,
	[Range(0, 100)] int? Accuracy,
	DateTime Date,
	[Range(typeof(decimal), "0.0000000000000000000000000001", "79228162514264337593543950335")] decimal ClosePrice,
	[StringLength(16)] string Signal
);
