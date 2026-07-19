using Application.Trades.Models;
using Domain.Trades.Entities;

namespace Application.Trades.Interfaces;

public interface ITradeCommandService
{
	Task<Trade> CreateAsync(int userId, TradeInput request, CancellationToken ct);
	Task DeleteAsync(int id, int userId, CancellationToken ct);
	Task UpdateAsync(int id, int userId, TradeInput request, CancellationToken ct);
}
