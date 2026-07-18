using Domain.Common;

namespace Domain.Strategies.Entities;

public sealed class TradeStrategy : AggregateRoot<int>
{
	public bool IsActive { get; private set; }

	public string Name { get; private set; }

	public string? Description { get; private set; }

	public int? Accuracy { get; private set; }

	public string? SignalFrequency { get; private set; }

	public string? InvestmentHorizon { get; private set; }

	public string? LogicDesc { get; private set; }

	public string? UseDesc { get; private set; }

	public string? LimitDesc { get; private set; }

	private readonly List<UserTradeStrategy> _userTradeStrategies = [];
	public IReadOnlyCollection<UserTradeStrategy> UserTradeStrategies => _userTradeStrategies.AsReadOnly();

	private TradeStrategy() { }

	public TradeStrategy(string name, string? description, int? accuracy, string? signalFrequency, string? investmentHorizon, string? logicDesc, string? useDesc, string? limitDesc)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Name cannot be empty.", nameof(name));

		Name = name;
		Description = description;
		Accuracy = accuracy;
		SignalFrequency = signalFrequency;
		InvestmentHorizon = investmentHorizon;
		LogicDesc = logicDesc;
		UseDesc = useDesc;
		LimitDesc = limitDesc;
	}

	public void SetActive(bool isActive)
	{
		IsActive = isActive;
	}
}
