using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Api.Contracts.Instruments;

public record InstrumentFileResponse(
	[Range(1, int.MaxValue)] int Id,
	[StringLength(255)] string Symbol,
	[StringLength(64)] string TimeFrame,
	DateTime StartDate,
	DateTime EndDate
);
