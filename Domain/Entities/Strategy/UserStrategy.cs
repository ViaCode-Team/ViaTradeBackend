namespace Domain.Entities;

public sealed class UserStrategy : BaseEntity<int>
{
	public required int UserId { get; set; }
	public required int StrategyId { get; set; }

	public User? User { get; set; }
	public Strategy? Strategy { get; set; }
}
