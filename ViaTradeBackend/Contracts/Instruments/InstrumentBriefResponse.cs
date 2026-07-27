using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Instruments;

public record InstrumentBriefResponse(
	[Range(1, int.MaxValue)] int Id,
	[StringLength(255)] string Symbol,
	string? Name
);
