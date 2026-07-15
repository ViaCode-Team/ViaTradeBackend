using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Domain.Entities.DataBase;
public class TradeStrategy : BaseEntity
{
	[MaxLength(128)]
	public required string Name { get; set; }
	[MaxLength(256)]
	public string? Description { get; set; }
	public int? Accuracy { get; set; }
	[MaxLength(128)]
	public string? SignalFrequency { get; set; }
	[MaxLength(128)]
	public string? InvestmentHorizon { get; set; }
	[MaxLength(256)]
	public string? LogicDesc { get; set; }
	[MaxLength(256)]
	public string? UseDesc { get; set; }
	[MaxLength(256)]
	public string? LimitDesc { get; set; }
	[JsonIgnore]
	public ICollection<UserTradeStrategy>? UserTradeStrategies { get; set; }
}

