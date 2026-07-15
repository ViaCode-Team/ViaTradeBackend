using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Domain.Entities.DataBase;
public class TradeType : BaseEntity
{
	[MaxLength(128)]
	public required string Name { get; set; }
	[JsonIgnore]
	public ICollection<Trade>? Trades { get; set; }
}

