namespace Application.Contracts.Dto.Requests.Trade;

public class UserStrategyTradeCodeCreateDto
{
	public required int TradeCodeId { get; set; }

	public required int StrategyId { get; set; }
}
