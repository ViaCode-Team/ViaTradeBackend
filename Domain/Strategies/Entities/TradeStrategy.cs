using Domain.Common;

namespace Domain.Strategies.Entities;

public sealed class TradeStrategy : BaseEntity<int>
{
	public bool IsActive { get; set; }

	public string Name { get; set; }

	public string? Description { get; set; }

	public int? Accuracy { get; set; }

	public string? SignalFrequency { get; set; }

	public string? InvestmentHorizon { get; set; }

	public string? LogicDesc { get; set; }

	public string? UseDesc { get; set; }

	public string? LimitDesc { get; set; }

	public ICollection<UserTradeStrategy> UserTradeStrategies { get; set; } = [];

}
