using Domain.Trades.Enums;
using Domain.Trades.Entities;
namespace Application.Contracts.Dto.Requests.Trade;

public record CreateUserStrategyCreateDto
{
	public required int StrategyId { get; set; }
}
