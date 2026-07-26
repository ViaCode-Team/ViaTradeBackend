namespace Application.Strategies.Interfaces;

public interface IStrategyCommandService
{
	Task LinkInstrumentAsync(int userId, int strategyId, int instrumentId, CancellationToken ct);
	Task ActivateAsync(int userId, int strategyId, CancellationToken ct);
	Task UnlinkInstrumentAsync(int userId, int strategyId, int instrumentId, CancellationToken ct);
	Task DeactivateAsync(int userId, int strategyId, CancellationToken ct);
}
