namespace ViaTrade.Domain.Entities;

public sealed class Strategy : BaseEntity<int>
{
	public required bool IsActive { get; set; }

	public required string Name { get; set; }

	public required string DisplayName { get; set; }

	public string? Description { get; set; }

	public int? Accuracy { get; set; }

	public string? SignalFrequency { get; set; }

	public string? InvestmentHorizon { get; set; }

	public string? LogicDescription { get; set; }

	public string? UsageDescription { get; set; }

	public string? LimitationsDescription { get; set; }

	public ICollection<UserStrategy> UserStrategies { get; set; } = [];
}
