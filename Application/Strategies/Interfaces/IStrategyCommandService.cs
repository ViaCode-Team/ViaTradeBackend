namespace Application.Strategies.Interfaces;

public interface IStrategyCommandService
{
	Task CreateCodeAsync(int userId, int strategyId, int tradeCodeId, CancellationToken ct);
	Task CreateAsync(int userId, int strategyId, CancellationToken ct);
	Task DeleteCodeAsync(int userId, int strategyId, int tradeCodeId, CancellationToken ct);
	Task DeleteAsync(int userId, int strategyId, CancellationToken ct);
}
