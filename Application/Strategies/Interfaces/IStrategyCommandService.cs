namespace ViaTrade.Application.Strategies.Interfaces;

public interface IStrategyCommandService
{
	Task LinkInstrumentAsync(int userId, int strategyId, int instrumentId, CancellationToken ct);
	Task SetSubscriptionAsync(int userId, int strategyId, bool isSubscribed, CancellationToken ct);
	Task UnlinkInstrumentAsync(int userId, int strategyId, int instrumentId, CancellationToken ct);
}
