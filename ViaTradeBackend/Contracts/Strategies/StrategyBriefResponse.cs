using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Strategies;

public record StrategyBriefResponse(
	[Range(1, int.MaxValue)] int Id,
	[StringLength(255)] string Name,
	string? Description
);
