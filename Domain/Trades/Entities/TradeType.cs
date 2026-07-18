using Domain.Trades.Enums;
using Domain.Trades.Entities;
using Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Trades.Entities;

public class TradeType : AggregateRoot
{
	[MaxLength(128)]
	public string Name { get; private set; }

	[JsonIgnore]
	public ICollection<Trade>? Trades { get; private set; }

	private TradeType() { }

	public TradeType(string name)
	{
		Name = name;
	}
}
