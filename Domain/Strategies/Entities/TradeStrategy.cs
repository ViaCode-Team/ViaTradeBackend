using Domain.Strategies.Entities;
using Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Strategies.Entities;

public class TradeStrategy : AggregateRoot
{
	[NotMapped]
	public bool IsActive { get; private set; }

	[MaxLength(128)]
	public string Name { get; private set; }

	[MaxLength(256)]
	public string? Description { get; private set; }

	public int? Accuracy { get; private set; }

	[MaxLength(128)]
	public string? SignalFrequency { get; private set; }

	[MaxLength(128)]
	public string? InvestmentHorizon { get; private set; }

	[MaxLength(256)]
	public string? LogicDesc { get; private set; }

	[MaxLength(256)]
	public string? UseDesc { get; private set; }

	[MaxLength(256)]
	public string? LimitDesc { get; private set; }

	[JsonIgnore]
	public ICollection<UserTradeStrategy>? UserTradeStrategies { get; private set; }

	private TradeStrategy() { }

	public TradeStrategy(string name, string? description, int? accuracy, string? signalFrequency, string? investmentHorizon, string? logicDesc, string? useDesc, string? limitDesc)
	{
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
