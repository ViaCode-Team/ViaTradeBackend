using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Domain.Entities.DataBase;
public class UserStrategyTradeCode : BaseEntity
{
	public required int UserId { get; set; }
	public required int TradeCodeId { get; set; }
	public required int StrategyId { get; set; }
	[JsonIgnore]
	public User? User { get; set; }
	[JsonIgnore]
	public TradeCode? TradeCode { get; set; }
	[JsonIgnore]
	public TradeStrategy? TradeStrategy { get; set; }
}

