using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.DataBase;

public class User : BaseEntity
{
	[Required]
	[MaxLength(64)]
	public required string Login { get; set; }

	[MaxLength(512)]
	public required string HashPassword { get; set; }

	public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;

	[Required]
	public required DateTime RegisterDate { get; set; }

	[MaxLength(512)]
	public string? TgId { get; set; }

	public ICollection<Trade>? Trades { get; set; }
	public ICollection<UserTradeCode>? UserTradeCodes { get; set; }
	public ICollection<UserTradeStrategy>? UserTradeStrategies { get; set; }
}
