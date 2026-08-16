namespace Application.Notes.Interfaces;

public interface INoteCommandService
{
	Task DeleteInstrumentAsync(int userId, int instrumentId, CancellationToken ct);
	Task DeleteStrategyAsync(int userId, int strategyId, CancellationToken ct);
	Task UpsertInstrumentAsync(int userId, int instrumentId, string text, CancellationToken ct);
	Task UpsertStrategyAsync(int userId, int strategyId, string text, CancellationToken ct);
}
