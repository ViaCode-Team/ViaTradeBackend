namespace Application.Strategies.Interfaces;

public interface IStrategyCommandService
{
	Task LinkInstrumentAsync(int userId, int strategyId, int instrumentId, CancellationToken ct);
	Task SetActivityAsync(int userId, int strategyId, bool isActive, CancellationToken ct);
	Task UnlinkInstrumentAsync(int userId, int strategyId, int instrumentId, CancellationToken ct);
}
