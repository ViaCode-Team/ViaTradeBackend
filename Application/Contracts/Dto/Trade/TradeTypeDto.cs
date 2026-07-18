using Domain.Trades.Enums;
using Domain.Trades.Entities;
namespace Application.Contracts.Dto.Trade;

public class TradeTypeDto
{
	public int Id { get; set; }
	public required string Name { get; set; }
}
