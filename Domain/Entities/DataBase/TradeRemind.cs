using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Entities.DataBase;

public class TradeRemind : BaseEntity
{
	[StringLength(1024)]
	public required string TextRemind { get; set; }
	public required DateTime DateTime { get; set; }
	public required int TradeCodeId { get; set; }
	public required int UserId { get; set; }
	[JsonIgnore]
	public TradeCode? TradeCode { get; set; }
	[JsonIgnore]
	public User? User { get; set; }
}
