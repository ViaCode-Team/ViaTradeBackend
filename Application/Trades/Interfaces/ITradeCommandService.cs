using Application.Trades.Models;

namespace Application.Trades.Interfaces;

public interface ITradeCommandService
{
	Task<TradeDto> CreateAsync(int userId, TradeInputDto request, CancellationToken ct);
	Task DeleteAsync(int id, int userId, CancellationToken ct);
	Task UpdateAsync(int id, int userId, TradeInputDto request, CancellationToken ct);
}
