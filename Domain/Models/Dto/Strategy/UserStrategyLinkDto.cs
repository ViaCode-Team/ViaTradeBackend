namespace Domain.Models.Dto.Strategy;

public class UserStrategyLinkDto
{
	public required int UserId { get; set; }

	public required int TradeStrategyId { get; set; }
}
