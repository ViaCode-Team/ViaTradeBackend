using Application.Trades.Models;

namespace Application.Trades.Interfaces;

public interface ITradeCommandService
{
	Task<TradeDto> CreateAsync(int userId, TradeInputDto request, CancellationToken ct);
	Task DeleteAsync(int userId, int id, CancellationToken ct);
	Task UpdateAsync(int userId, int id, TradeInputDto request, CancellationToken ct);
}
