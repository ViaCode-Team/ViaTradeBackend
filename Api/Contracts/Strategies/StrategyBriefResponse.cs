using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Api.Contracts.Strategies;

public record StrategyBriefResponse(
	[Range(1, int.MaxValue)] int Id,
	[StringLength(255)] string Name,
	[StringLength(255)] string DisplayName,
	string? Description
);
