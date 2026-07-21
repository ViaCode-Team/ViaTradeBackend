using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Trades.Models;
using Domain.Entities;

namespace Application.Trades.Interfaces;

public interface ITradeRepository : IRepository<Trade>
{
	Task<TradeStatisticAggregateDto> GetGlobalStatisticAsync(int userId, CancellationToken ct = default);
	Task<int> ExecuteUpdateAsync(
		int id,
		int userId,
		TradeInputDto request,
		decimal price,
		CancellationToken ct = default
	);
}
