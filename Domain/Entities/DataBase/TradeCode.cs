using Domain.Trades.Enums;
using Domain.Trades.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Entities.DataBase;

public class TradeCode : BaseEntity
{
	[MaxLength(128)]
	public required string ExchangeId { get; set; }
	[MaxLength(512)]
	public string? Description { get; set; }
	[JsonIgnore]
	public ICollection<Trade>? Trades { get; set; }
	[JsonIgnore]
	public ICollection<UserTradeCode>? UserTradeCodes { get; set; }
}
