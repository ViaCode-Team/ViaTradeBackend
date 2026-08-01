using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Instruments;

public record InstrumentResponse(
	[Range(1, int.MaxValue)] int Id,
	[StringLength(255)] string Symbol,
	string? Description
);
