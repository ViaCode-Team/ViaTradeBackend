using ViaTrade.Application.Notes.Models;

namespace ViaTrade.Application.Notes.Interfaces;

public interface INoteRepository
{
	Task<NoteStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct = default);
	Task<int> ExecuteUpdateInstrumentAsync(int userId, int instrumentId, string text, CancellationToken ct = default);
	Task<int> ExecuteUpdateStrategyAsync(int userId, int strategyId, string text, CancellationToken ct = default);
}
