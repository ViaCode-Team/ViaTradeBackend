using ViaTrade.Application.Notes.GetStatistics;

namespace ViaTrade.Application.Notes.Common.Abstractions;

public interface INoteRepository
{
	Task<NoteStatisticsResult> GetStatisticsAsync(int userId, CancellationToken ct = default);
	Task<int> ExecuteUpdateInstrumentAsync(int userId, int instrumentId, string text, CancellationToken ct = default);
	Task<int> ExecuteUpdateStrategyAsync(int userId, int strategyId, string text, CancellationToken ct = default);
}
